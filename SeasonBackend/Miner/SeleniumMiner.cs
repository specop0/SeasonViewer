using HtmlAgilityPack;
using SeasonBackend.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

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

                var anime = new Anime
                {
                    Season = season,
                    Mal = new MalInformation
                    {
                        Id = malId.ToString(),
                        Name = title,
                        ImageUrl = imageUrl,
                        MemberCount = memberCount,
                        Score = score,
                    }
                };
                animes.Add(anime);
            }

            return animes.ToArray();
        }

        public MineHosterResult MineHoster(Anime anime)
        {
            var result = new MineHosterResult();

            var hosters = new List<HosterInformation>();
            hosters.AddRange(this.ParseAmazon(anime));

            return result;
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
            var searchResults = searchResultDiv.SelectNodes("div");

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
    }
}
