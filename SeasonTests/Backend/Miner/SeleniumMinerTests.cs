namespace SeasonTests.Backend.Miner
{
    using Microsoft.Extensions.Configuration;
    using NUnit.Framework;
    using SeasonBackend.Database;
    using SeasonBackend.Miner;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;

    public class SeleniumMinerTests : TestBase
    {
        protected string GetResource(params string[] path)
        {
            var name = this.GetType().Namespace + "." + string.Join(".", path);
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                ?? throw new Exception($"Could find the following resource: {path}.");
            var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return content;
        }

        [Test]
        public void TestParseSingleAnimeFromJson()
        {
            var input = this.GetResource("anime.json");
            var actualAnime = SeleniumMiner.ParseAnime(input);

            Assert.That(actualAnime, Is.Not.Null);
            Assert.That(actualAnime.Mal.Name, Is.EqualTo("Boku no Hero Academia 4th Season"));
            Assert.That(actualAnime.Mal.Id, Is.EqualTo("38408"));
            Assert.That(actualAnime.Mal.EpisodesCount, Is.EqualTo(25));
            Assert.That(actualAnime.Mal.Score, Is.EqualTo(813));
            Assert.That(actualAnime.Mal.MemberCount, Is.EqualTo(742500));
            Assert.That(actualAnime.Mal.ImageUrl, Is.EqualTo("https://cdn.myanimelist.net/images/anime/1315/102961.jpg"));
        }

        [Test]
        public void TestParseAnimeFromJson()
        {
            var input = this.GetResource("mal-season.json");
            var season = this.GetUniqueName();
            var actualAnimes = SeleniumMiner.ParseSeasonAnime(input, season);
            Assert.That(actualAnimes, Is.Not.Empty);

            var expectedAnimes = JsonSerializer.Deserialize<Anime[]>(this.GetResource("mal-season-expected.json"));
            Assert.That(expectedAnimes, Is.Not.Empty, "unit test implementation");
            // System.IO.File.WriteAllText(@"mal-season-expected.json", JsonSerializer.Serialize(actualAnimes, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));

            Assert.That(actualAnimes.Length, Is.EqualTo(expectedAnimes.Length));

            for (var i = 0; i < expectedAnimes.Length; i++)
            {
                var expected = expectedAnimes[i];
                var actual = actualAnimes[i];

                Assert.That(actual.Mal.Id, Is.EqualTo(expected.Mal.Id));
                Assert.That(actual.Mal.Name, Is.EqualTo(expected.Mal.Name));
                Assert.That(actual.Mal.ImageUrl, Is.EqualTo(expected.Mal.ImageUrl));
                Assert.That(actual.Mal.MemberCount, Is.EqualTo(expected.Mal.MemberCount));
                Assert.That(actual.Mal.Score, Is.EqualTo(expected.Mal.Score));

                Assert.That(actual.Seasons, Is.EqualTo(new[] { season }));
                Assert.That(actual.Id, Is.EqualTo(default(int)));

                Assert.That(actual.Hoster, Is.Empty);

                Assert.That(expected.Mal.ImageUrl, Is.Not.Null, $"missing image for {expected.Mal.Name}");
            }
        }

        [Test]
        public void TestParseDuckDuckGoSearchFromJson()
        {
            var input = this.GetResource("duckduckgo-search.json");
            var searchResult = SeleniumMiner.ParseDuckDuckGoSearch(input);

            var expectedResults = JsonSerializer.Deserialize<DuckDuckGoSearchItem[]>(this.GetResource("duckduckgo-search-expected.json"));
            Assert.That(expectedResults, Is.Not.Empty, "unit test implementation");
            Assert.That(searchResult.Length, Is.EqualTo(expectedResults.Length));

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var expected = expectedResults[i];
                var actual = searchResult[i];

                Assert.That(actual.Name, Is.EqualTo(expected.Name));
                Assert.That(actual.Url, Is.EqualTo(expected.Url));
            }
        }

        [Test]
        public void TestParseMalListFromJson()
        {
            var input = this.GetResource("mal-list.json");
            var listEntries = SeleniumMiner.ParseMalList(input);

            Assert.That(listEntries.Count(), Is.GreaterThan(1000));

            var expectedResults = JsonSerializer.Deserialize<MalListMineResult[]>(this.GetResource("mal-list-expected.json"));
            Assert.That(expectedResults, Is.Not.Empty, "unit test implementation");
            Assert.That(listEntries.Length, Is.EqualTo(expectedResults.Length));

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var expected = expectedResults[i];
                var actual = listEntries[i];

                Assert.That(actual.AnimeId, Is.EqualTo(expected.AnimeId));
                Assert.That(actual.Status, Is.EqualTo(expected.Status));
            }

            var groupedByStatus = listEntries.GroupBy(x => x.Status).ToDictionary(x => x.Key, x => x.ToArray());
            var everyValidStatus = Enum.GetValues(typeof(ListStatus)).Cast<ListStatus>().Except(new[] { ListStatus.Unknown });
            Assert.That(groupedByStatus.Keys, Is.EquivalentTo(everyValidStatus));
            Assert.That(groupedByStatus.All(x => x.Value.Any()), Is.True);
        }

        [Test, Ignore("Miner must be listening")]
        public void TestParseAnime()
        {
            var season = "2020/summer";
            var configurationDictionary = new Dictionary<string, string?>
            {
                { "ConnectionStrings:SeleniumMinerUrl", "http://localhost:22471/mine/" },
            };
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder.AddInMemoryCollection(configurationDictionary).Build();
            var testee = new SeleniumMiner(new HttpClient(), configuration);
            var animes = testee.MineSeasonAnimeAsync(season).Result.Animes;
            Assert.That(animes, Is.Not.Empty);
            System.IO.File.WriteAllText(
                "expected.json",
                JsonSerializer.Serialize(animes, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
        }
    }
}
