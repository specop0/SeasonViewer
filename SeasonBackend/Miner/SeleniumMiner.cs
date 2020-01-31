using HtmlAgilityPack;
using SeasonBackend.Database;
using SeasonBackend.Protos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace SeasonBackend.Miner
{
    public class SeleniumMiner
    {
        private HttpClient miner;
        protected HttpClient Miner
        {
            get
            {
                if (this.miner == null)
                {
                    this.miner = new HttpClient();
                    this.miner.BaseAddress = new Uri(Environment.GetEnvironmentVariable("seleniumMinerUrl"));
                    this.miner.Timeout = TimeSpan.FromMinutes(15d);
                }
                return this.miner;
            }
        }

        public SeasonAnimeMineResult MineSeasonAnime(string season)
        {
            var result = new SeasonAnimeMineResult();

            var seasonUrl = new Uri("https://myanimelist.net/anime/season/" + season);
            var pageSourceRequest = new MinePageSourceRequest
            {
                Url = seasonUrl.ToString()
            };

            var response = this.Miner.PostAsync("pageSource", pageSourceRequest.ToHttpContent()).Result;
            if (response.IsSuccessStatusCode)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                result.Animes = ParseSeasonAnime(body, season);
            }

            return result;
        }

        public static Anime[] ParseSeasonAnime(string body, string season)
        {
            var pageSourceResult = body.Deserialize<MinePageSourceResult>();
            var pageSource = pageSourceResult.PageSource;

            var animes = new List<Anime>();

            var document = new HtmlDocument();
            document.LoadHtml(pageSource);

            var animeNodes = document.DocumentNode.SelectNodes("//div[@class='seasonal-anime js-seasonal-anime']");

            foreach (var animeNode in animeNodes)
            {
                var titleElement = animeNode.SelectSingleNode("div/div[@class='title']/p/a");

                var title = titleElement.InnerText;
                var malUrl = titleElement.GetAttributeValue("href", "");
                var malId = malUrl.Split('/').Select(x =>
                {
                    if (int.TryParse(x, out var id))
                    {
                        return id;
                    }

                    return -1;
                }).FirstOrDefault(x => x > 0);

                var imageElement = animeNode.SelectSingleNode("div[@class='image']/a/img");
                var imageUrl = imageElement.GetAttributeValue("src", "");
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = imageElement.GetAttributeValue("data-src", "");
                }

                var scoreAndMemberDiv = animeNode.SelectSingleNode("div[@class='information']/div[@class='scormem']");
                var memberDiv = scoreAndMemberDiv.SelectSingleNode("span[@title='Members']");
                ulong memberCount = 0L;
                if (double.TryParse(memberDiv.InnerText, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var memberCountDouble))
                {
                    memberCount = (ulong)memberCountDouble;
                }
                var scoreDiv = scoreAndMemberDiv.SelectSingleNode("span[@title='Score']");
                uint score = 0;
                if (double.TryParse(scoreDiv.InnerText, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var scoreDouble))
                {
                    score = (uint)(scoreDouble * 100d);
                }

                ulong episodesCount = 0L;
                var episodesCountSpan = animeNode.SelectSingleNode(".//div[@class='eps']/a/span");
                if (!string.IsNullOrEmpty(episodesCountSpan.InnerText))
                {
                    var number = episodesCountSpan.InnerText.Split(" ").FirstOrDefault();
                    if (ulong.TryParse(number, out var episodesCountParsed))
                    {
                        episodesCount = episodesCountParsed;
                    }
                }

                var anime = new Anime
                {
                    Seasons = new[] { season },
                    Mal = new MalInformation
                    {
                        Id = malId.ToString(),
                        Name = title,
                        ImageUrl = imageUrl,
                        MemberCount = memberCount,
                        Score = score,
                        EpisodesCount = episodesCount,
                    }
                };
                animes.Add(anime);
            }

            return animes.ToArray();
        }

        public MalListMineResult[] MineMalList(string user)
        {
            var result = new MalListMineResult[0];

            var seasonUrl = new Uri("https://myanimelist.net/animelist/" + user);
            var pageSourceRequest = new MinePageSourceRequest
            {
                Url = seasonUrl.ToString()
            };

            var response = this.Miner.PostAsync("pageSource", pageSourceRequest.ToHttpContent()).Result;
            if (response.IsSuccessStatusCode)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                result = ParseMalList(body);
            }

            return result;
        }

        public static MalListMineResult[] ParseMalList(string body)
        {
            var pageSourceResult = body.Deserialize<MinePageSourceResult>();
            var pageSource = pageSourceResult.PageSource;

            var animes = new List<MalListMineResult>();

            var document = new HtmlDocument();
            document.LoadHtml(pageSource);

            var tables = document.DocumentNode.SelectNodes("//table");

            var currentStatus = ListStatus.Unknown;
            foreach (var table in tables)
            {
                var tableClass = table.GetAttributeValue("class", "");
                switch (tableClass)
                {
                    case "header_cw":
                        currentStatus = ListStatus.Watching;
                        break;
                    case "header_completed":
                        currentStatus = ListStatus.Completed;
                        break;
                    case "header_onhold":
                        currentStatus = ListStatus.OnHold;
                        break;
                    case "header_dropped":
                        currentStatus = ListStatus.Dropped;
                        break;
                    case "header_ptw":
                        currentStatus = ListStatus.Plan2Watch;
                        break;
                }

                var animeTitle = table.SelectSingleNode(".//a[@class='animetitle']");

                if (animeTitle != null)
                {
                    var animeLink = animeTitle.GetAttributeValue("href", "");
                    var animeId = animeLink.Substring("/anime/".Length).Split("/").FirstOrDefault();

                    if (!string.IsNullOrEmpty(animeId))
                    {
                        animes.Add(new MalListMineResult
                        {
                            AnimeId = animeId,
                            Status = currentStatus
                        });
                    }
                }
            }

            return animes.ToArray();
        }

        public MineHosterResult MineHoster(Anime anime)
        {
            var result = new MineHosterResult();

            var hosters = new List<HosterInformation>();

            //hosters.AddRange(this.ParseAmazon(anime));
            // amazon
            {
                var searchResults = this.ParseDuckDuckGo($"site:amazon.de {anime.Mal.Name} prime video");
                var midfix = "/dp/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.Contains(midfix));
                if (searchResult != null)
                {
                    var id = searchResult.Url.Substring(searchResult.Url.IndexOf(midfix) + midfix.Length).Split("/").FirstOrDefault();
                    hosters.Add(new HosterInformation
                    {
                        HosterType = HosterType.Amazon,
                        Id = id,
                        Name = searchResult.Name,
                        Url = searchResult.Url,
                    });
                }
            }

            // wakanim.tv
            {
                var searchResults = this.ParseDuckDuckGo($"site:wakanim.tv {anime.Mal.Name}");
                var prefix = "https://www.wakanim.tv/de/v2/catalogue/show/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.StartsWith(prefix));
                if (searchResult != null)
                {
                    var id = searchResult.Url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    hosters.Add(new HosterInformation
                    {
                        HosterType = HosterType.Wakanim,
                        Id = id,
                        Name = searchResult.Name,
                        Url = searchResult.Url,
                    });
                }
            }

            // netflix
            {
                var searchResults = this.ParseDuckDuckGo($"site:netflix.com {anime.Mal.Name}");
                var prefix = "https://www.netflix.com/title/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.StartsWith(prefix));
                if (searchResult != null)
                {
                    var id = searchResult.Url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    hosters.Add(new HosterInformation
                    {
                        HosterType = HosterType.Netflix,
                        Id = id,
                        Name = searchResult.Name,
                        Url = searchResult.Url,
                    });
                }
            }

            // crunchyroll
            {
                var searchResults = this.ParseDuckDuckGo($"site:crunchyroll.com {anime.Mal.Name}");
                var prefix = "https://www.crunchyroll.com/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.StartsWith(prefix) && x.Url.Substring(prefix.Length).Split("/").Length == 1);
                if (searchResult != null)
                {
                    var id = searchResult.Url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    hosters.Add(new HosterInformation
                    {
                        HosterType = HosterType.Crunchyroll,
                        Id = id,
                        Name = searchResult.Name,
                        Url = searchResult.Url,
                    });
                }
            }

            // anime-on-demand
            {
                var searchResults = this.ParseDuckDuckGo($"site:anime-on-demand.de {anime.Mal.Name}");
                var prefix = "https://www.anime-on-demand.de/anime/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.StartsWith(prefix));
                if (searchResult != null)
                {
                    var id = searchResult.Url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    hosters.Add(new HosterInformation
                    {
                        HosterType = HosterType.AnimeOnDemand,
                        Id = id,
                        Name = searchResult.Name,
                        Url = searchResult.Url,
                    });
                }
            }

            result.Hosters = hosters.ToArray();
            return result;
        }

        public HosterInformation[] ParseHoster(Anime anime, IEnumerable<Hoster> hostersInput)
        {
            var hosters = new List<HosterInformation>();

            foreach (var hosterInput in hostersInput)
            {
                var url = hosterInput.Url;
                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                var hoster = new HosterInformation
                {
                    Url = url,
                    Name = anime.Mal.Name,
                    HosterType = hosterInput.HosterType,
                    Id = Guid.NewGuid().ToString()
                };
                if (!string.IsNullOrEmpty(hosterInput.Name))
                {
                    hoster.Name = hosterInput.Name;
                }
                if (!string.IsNullOrEmpty(hosterInput.Id))
                {
                    hoster.Id = hosterInput.Id;
                }

                // amazon
                {
                    var prefix = "https://www.amazon.de/";
                    if (url.StartsWith(prefix))
                    {
                        hoster.HosterType = HosterType.Amazon;

                        var midfix = "/dp/";
                        if (url.Contains(midfix))
                        {
                            var id = url.Substring(url.IndexOf(midfix) + midfix.Length).Split("/").FirstOrDefault();
                            if (!string.IsNullOrEmpty(id))
                            {
                                hoster.Id = id;
                            }
                        }
                    }
                }

                // wakanim.tv
                {
                    var prefix = "https://www.wakanim.tv/de/v2/catalogue/show/";
                    if (url.StartsWith(prefix))
                    {
                        hoster.HosterType = HosterType.Wakanim;
                        hoster.Id = url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    }
                }

                // netflix
                {
                    var prefix = "https://www.netflix.com/de/title/";
                    if (url.StartsWith(prefix))
                    {
                        hoster.HosterType = HosterType.Netflix;
                        hoster.Id = url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    }
                }

                // crunchyroll
                {
                    var prefix = "https://www.crunchyroll.com/de/";
                    if (url.StartsWith(prefix))
                    {
                        hoster.HosterType = HosterType.Crunchyroll;
                        hoster.Id = url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    }
                }

                // anime-on-demand
                {
                    var prefix = "https://www.anime-on-demand.de/anime/";
                    if (url.StartsWith(prefix))
                    {
                        hoster.HosterType = HosterType.AnimeOnDemand;
                        hoster.Id = url.Substring(prefix.Length).Split("/").FirstOrDefault();
                    }
                }

                hosters.Add(hoster);
            }

            return hosters.ToArray();
        }

        public HosterInformation[] ParseAmazon(Anime anime)
        {
            var request = new AmazonSearchRequest
            {
                Url = "https://www.amazon.de/gp/video/storefront?filterId=OFFER_FILTER%3DPRIME",
                Search = anime.Mal.Name,
            };

            var response = this.Miner.PostAsync("amazon", request.ToHttpContent()).Result;
            if (response.IsSuccessStatusCode)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                return ParseAmazonSearch(anime, body);
            }

            return new HosterInformation[0];
        }

        public static HosterInformation[] ParseAmazonSearch(Anime anime, string body)
        {
            var hosterInformations = new List<HosterInformation>();

            var pageSourceResult = body.Deserialize<AmazonSearchResult>();
            var pageSource = pageSourceResult.PageSource;

            var document = new HtmlDocument();
            document.LoadHtml(pageSource);

            var searchResultDiv = document.DocumentNode.SelectSingleNode("//div[@class='s-result-list s-search-results sg-row']");
            IEnumerable<HtmlNode> searchResults = searchResultDiv.SelectNodes("div") ?? Enumerable.Empty<HtmlNode>();

            foreach (var searchResult in searchResults)
            {
                var amazonId = searchResult.GetAttributeValue("data-asin", "");
                var amazonName = searchResult.SelectSingleNode(".//img")?.GetAttributeValue("alt", "");

                if (string.IsNullOrEmpty(amazonId) || string.IsNullOrEmpty(amazonName))
                {
                    continue;
                }

                var hosterInformation = new HosterInformation
                {
                    HosterType = Protos.HosterType.Amazon,
                    Id = amazonId,
                    Name = amazonName,
                    Url = $"https://www.amazon.de/dp/{amazonId}",
                };
                hosterInformations.Add(hosterInformation);
            }

            if (hosterInformations.Count > 1)
            {
                var simplifySearchRegex = new Regex("^[A-z,0-9]");
                var name = simplifySearchRegex.Replace(anime.Mal.Name, "").ToLowerInvariant();

                var matchingHosterInformations = hosterInformations.Where(x =>
                {
                    var amazonName = simplifySearchRegex.Replace(x.Name, "").ToLowerInvariant();
                    return name == amazonName;
                }).ToList();

                if (matchingHosterInformations.Count == 1)
                {
                    return matchingHosterInformations.ToArray();
                }
            }


            return hosterInformations.ToArray();
        }

        public DuckDuckGoSearchItem[] ParseDuckDuckGo(string searchQuery)
        {
            var request = new DuckDuckGoSearchRequest
            {
                Search = searchQuery,
            };

            var response = this.Miner.PostAsync("duckduckgo", request.ToHttpContent()).Result;
            if (response.IsSuccessStatusCode)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                return ParseDuckDuckGoSearch(body);
            }

            return new DuckDuckGoSearchItem[0];
        }

        public static DuckDuckGoSearchItem[] ParseDuckDuckGoSearch(string body)
        {
            var result = new List<DuckDuckGoSearchItem>();

            var pageSourceResult = body.Deserialize<DuckDuckGoSearchResult>();
            var pageSource = pageSourceResult.PageSource;

            var document = new HtmlDocument();
            document.LoadHtml(pageSource);

            var searchResultDiv = document.DocumentNode.SelectSingleNode("//div[@class='results js-results']");
            IEnumerable<HtmlNode> searchResults = searchResultDiv.SelectNodes("div") ?? Enumerable.Empty<HtmlNode>();

            foreach (var searchResult in searchResults)
            {
                var resultLink = searchResult.SelectSingleNode(".//a");
                if (resultLink == null)
                {
                    continue;
                }

                var href = resultLink.GetAttributeValue("href", "");
                var name = resultLink.InnerText;
                if (string.IsNullOrEmpty(href) || string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var resultItem = new DuckDuckGoSearchItem
                {
                    Name = name,
                    Url = href,
                };
                result.Add(resultItem);
            }

            return result.ToArray();
        }
    }
}
