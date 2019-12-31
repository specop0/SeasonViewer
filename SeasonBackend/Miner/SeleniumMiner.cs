using HtmlAgilityPack;
using SeasonBackend.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
            var json = JsonSerializer.Serialize(pageSourceRequest);

            var response = this.Miner.PostAsync("pageSource", new StringContent(json, Encoding.UTF8, "application/json")).Result;
            if (response.IsSuccessStatusCode)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                result.Animes = ParseSeasonAnime(body, season);
            }

            return result;
        }

        public static Anime[] ParseSeasonAnime(string body, string season)
        {
            var options = new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var pageSourceResult = JsonSerializer.Deserialize<MinePageSourceResult>(body, options);
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
    }
}
