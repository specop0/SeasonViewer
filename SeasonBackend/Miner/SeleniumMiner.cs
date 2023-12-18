using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using SeasonBackend.Database;
using SeasonBackend.Protos;

namespace SeasonBackend.Miner
{
    public class SeleniumMiner
    {
        public SeleniumMiner(HttpClient httpClient, IConfiguration configuration)
        {
            var url = configuration.GetValue<string>("ConnectionStrings:SeleniumMinerUrl");
            httpClient.BaseAddress = new Uri(url);
            httpClient.Timeout = TimeSpan.FromMinutes(15d);
            this.Miner = httpClient;
        }

        protected HttpClient Miner { get; }

        public Anime[] MineAnimes(ICollection<Anime> animes)
        {
            var foundAnimes = new List<Anime>();

            foreach (var anime in animes)
            {
                var newAnime = this.MineAnime(anime.Mal.Id);
                if (newAnime != null)
                {
                    newAnime.Mal.Status = anime.Mal.Status;
                    foundAnimes.Add(newAnime);
                }
            }

            return foundAnimes.ToArray();
        }

        private Anime MineAnime(string id)
        {
            Anime anime = null;
            var requestUrl = new Uri($"https://myanimelist.net/anime/{id}");


            var pageSourceRequest = new MinePageSourceRequest
            {
                Url = requestUrl.ToString()
            };

            var response = this.Miner.PostAsync("pageSource", pageSourceRequest.ToHttpContent()).Result;
            if (response.IsSuccessStatusCode)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                anime = ParseAnime(body);
            }

            return anime;
        }

        public static Anime ParseAnime(string body)
        {
            Anime anime = null;

            var pageSourceResult = body.Deserialize<MinePageSourceResult>();
            var pageSource = pageSourceResult.PageSource;

            var document = new HtmlDocument();
            document.LoadHtml(pageSource);

            var contentWrapper = document.DocumentNode.SelectSingleNode("//div[@id='contentWrapper']");

            var animeName = contentWrapper.SelectSingleNode(".//h1[@class='title-name h1_bold_none']/strong").GetDirectInnerText();

            var malId = contentWrapper.SelectSingleNode(".//input[@id='myinfo_anime_id']").GetAttributeValue("value", "");

            var border = contentWrapper.SelectSingleNode(".//td[@class='borderClass']/div");
            var imageElement = border.SelectSingleNode(".//img");
            var imageUrl = imageElement.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = imageElement.GetAttributeValue("data-src", "");
            }

            var episodesCountRow = border.SelectSingleNode(".//span[text()='Episodes:']");
            episodesCountRow = episodesCountRow.ParentNode.ChildNodes[2];
            var episodesCountText = episodesCountRow.GetDirectInnerText()?.Replace(" ", "")?.Replace("\n", "");
            ulong episodesCount = 0L;
            if (!string.IsNullOrEmpty(episodesCountText))
            {
                if (ulong.TryParse(episodesCountText, out var episodesCountParsed))
                {
                    episodesCount = episodesCountParsed;
                }
            }

            var statisticsHeader = border.SelectSingleNode("./h2[text()='Statistics']");
            var statisticsIndex = border.ChildNodes.IndexOf(statisticsHeader);

            var scoreRow = border.ChildNodes.ElementAt(statisticsIndex + 2);
            var scoreTextElement = scoreRow.SelectSingleNode("./span[@itemprop='ratingValue']");
            uint score = 0;
            if (scoreTextElement != null)
            {
                var scoreText = scoreTextElement.GetDirectInnerText();
                if (double.TryParse(scoreText, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var scoreDouble))
                {
                    score = (uint)(scoreDouble * 100d);
                }
            }

            var memberCountRow = border.ChildNodes.ElementAt(statisticsIndex + 8);
            var memberCountText = memberCountRow.GetDirectInnerText()?.Replace(" ", "")?.Replace("\n", "");
            ulong memberCount = 0L;
            if (!string.IsNullOrEmpty(memberCountText))
            {
                if (double.TryParse(memberCountText, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var memberCountDouble))
                {
                    memberCount = (ulong)memberCountDouble;
                }
            }

            if (!string.IsNullOrEmpty(malId))
            {
                anime = new Anime
                {
                    Seasons = new List<string>(),
                    Mal = new MalInformation
                    {
                        Id = malId,
                        Name = animeName,
                        ImageUrl = imageUrl,
                        MemberCount = memberCount,
                        Score = score,
                        EpisodesCount = episodesCount,
                    }
                };
            }

            return anime;
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

            var animeNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'seasonal-anime js-seasonal-anime')]");

            foreach (var animeNode in animeNodes)
            {
                var titleElement = animeNode.SelectSingleNode(".//a[contains(@class, 'link-title')]");

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

                var imageElement = animeNode.SelectSingleNode(".//img");
                var imageUrl = imageElement.GetAttributeValue("src", "");
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = imageElement.GetAttributeValue("data-src", "");
                }

                var scoreAndMemberDiv = animeNode.SelectSingleNode(".//div[contains(@class, 'scormem')]");
                var memberDiv = scoreAndMemberDiv.SelectSingleNode(".//div[contains(@title, 'Members')]");
                ulong memberCount = 0L;
                var membersText = memberDiv.GetDirectInnerText();
                ulong membersCountScale = 1L;
                if (membersText.Contains('K'))
                {
                    membersText = membersText.Replace("K", "");
                    membersCountScale = 1000L;
                }
                if (double.TryParse(membersText, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var memberCountDouble))
                {
                    memberCount = (ulong)memberCountDouble;
                    memberCount *= membersCountScale;
                }
                var scoreDiv = scoreAndMemberDiv.SelectSingleNode(".//div[contains(@title, 'Score')]");
                uint score = 0;
                if (double.TryParse(scoreDiv.InnerText, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var scoreDouble))
                {
                    score = (uint)(scoreDouble * 100d);
                }

                ulong episodesCount = 0L;
                var episodesCountSpan = animeNode.SelectSingleNode(".//div[contains(@class, 'prodsrc')]/div[@class='info']/span[2]/span[1]");
                if (!string.IsNullOrEmpty(episodesCountSpan.InnerText))
                {
                    var number = episodesCountSpan.InnerText.Split(" ").FirstOrDefault();
                    if (ulong.TryParse(number, out var episodesCountParsed))
                    {
                        episodesCount = episodesCountParsed;
                    }
                }

                var hentaiGenre = animeNode.SelectSingleNode(".//span[contains(@class, 'genre')]/a[@title='Hentai']");
                if (hentaiGenre != null)
                {
                    continue;
                }

                var anime = new Anime
                {
                    Seasons = new List<string> { season },
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
            var result = Array.Empty<MalListMineResult>();

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
                    var animeId = animeLink["/anime/".Length..].Split("/").FirstOrDefault();

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

            // wakanim.tv
            {
                var searchResults = this.ParseDuckDuckGo($"site:wakanim.tv {anime.Mal.Name}");
                var prefix = "https://www.wakanim.tv/de/v2/catalogue/show/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.StartsWith(prefix));
                if (searchResult != null)
                {
                    hosters.Add(new HosterInformation
                    {
                        Name = searchResult.Name,
                        Url = searchResult.Url,
                    });
                }
            }

            // crunchyroll
            {
                var searchResults = this.ParseDuckDuckGo($"site:crunchyroll.com {anime.Mal.Name}");
                var prefix = "https://www.crunchyroll.com/";
                var searchResult = searchResults.Take(7).FirstOrDefault(x => x.Url.StartsWith(prefix) && x.Url[prefix.Length..].Split("/").Length == 1);
                if (searchResult != null)
                {
                    hosters.Add(new HosterInformation
                    {
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
                };

                if (!string.IsNullOrEmpty(hosterInput.Name))
                {
                    hoster.Name = hosterInput.Name;
                }

                hosters.Add(hoster);
            }

            return hosters.ToArray();
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

            return Array.Empty<DuckDuckGoSearchItem>();
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
