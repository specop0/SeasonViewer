namespace SeasonTests.Backend.Miner
{
    using NUnit.Framework;
    using SeasonBackend.Database;
    using SeasonBackend.Miner;
    using SeasonBackend.Protos;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    public class SeleniumMinerTests : TestBase
    {
        protected string GetResource(params string[] path)
        {
            var name = this.GetType().Namespace + "." + string.Join(".", path);
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return content;
        }

        [Test]
        public void TestParseSingleAnimeFromJson()
        {
            var input = this.GetResource("anime.json");
            var actualAnime = SeleniumMiner.ParseAnime(input);

            Assert.IsNotNull(actualAnime);
            Assert.AreEqual("Boku no Hero Academia 4th Season", actualAnime.Mal.Name);
            Assert.AreEqual("38408", actualAnime.Mal.Id);
            Assert.AreEqual(25, actualAnime.Mal.EpisodesCount);
            Assert.AreEqual(813, actualAnime.Mal.Score);
            Assert.AreEqual(742500, actualAnime.Mal.MemberCount);
            Assert.AreEqual("https://cdn.myanimelist.net/images/anime/1315/102961.jpg", actualAnime.Mal.ImageUrl);
        }

        [Test]
        public void TestParseAnimeFromJson()
        {
            var input = this.GetResource("mal-season.json");
            var season = this.GetUniqueName();
            var actualAnimes = SeleniumMiner.ParseSeasonAnime(input, season);
            CollectionAssert.IsNotEmpty(actualAnimes);

            var expectedAnimes = JsonSerializer.Deserialize<Anime[]>(this.GetResource("mal-season-expected.json"));
            CollectionAssert.IsNotEmpty(expectedAnimes, "unit test implementation");
            // System.IO.File.WriteAllText(@"mal-season-expected.json", JsonSerializer.Serialize(actualAnimes, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));

            Assert.AreEqual(expectedAnimes.Length, actualAnimes.Length);

            for (var i = 0; i < expectedAnimes.Length; i++)
            {
                var expected = expectedAnimes[i];
                var actual = actualAnimes[i];

                Assert.AreEqual(expected.Mal.Id, actual.Mal.Id);
                Assert.AreEqual(expected.Mal.Name, actual.Mal.Name);
                Assert.AreEqual(expected.Mal.ImageUrl, actual.Mal.ImageUrl);
                Assert.AreEqual(expected.Mal.MemberCount, actual.Mal.MemberCount);
                Assert.AreEqual(expected.Mal.Score, actual.Mal.Score);

                CollectionAssert.AreEqual(new[] { season }, actual.Seasons);
                Assert.AreEqual(default(int), actual.Id);

                CollectionAssert.IsEmpty(actual.Hoster);

                Assert.IsNotNull(expected.Mal.ImageUrl, $"missing image for {expected.Mal.Name}");
            }
        }

        [Test]
        public void TestParseDuckDuckGoSearchFromJson()
        {
            var input = this.GetResource("duckduckgo-search.json");
            var searchResult = SeleniumMiner.ParseDuckDuckGoSearch(input);

            var expectedResults = JsonSerializer.Deserialize<DuckDuckGoSearchItem[]>(this.GetResource("duckduckgo-search-expected.json"));
            CollectionAssert.IsNotEmpty(expectedResults, "unit test implementation");
            Assert.AreEqual(expectedResults.Length, searchResult.Length);

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var expected = expectedResults[i];
                var actual = searchResult[i];

                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.Url, actual.Url);
            }
        }

        [Test]
        public void TestParseMalListFromJson()
        {
            var input = this.GetResource("mal-list.json");
            var listEntries = SeleniumMiner.ParseMalList(input);

            Assert.Greater(listEntries.Count(), 1000);

            var expectedResults = JsonSerializer.Deserialize<MalListMineResult[]>(this.GetResource("mal-list-expected.json"));
            CollectionAssert.IsNotEmpty(expectedResults, "unit test implementation");
            Assert.AreEqual(expectedResults.Length, listEntries.Length);

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var expected = expectedResults[i];
                var actual = listEntries[i];

                Assert.AreEqual(expected.AnimeId, actual.AnimeId);
                Assert.AreEqual(expected.Status, actual.Status);
            }

            var groupedByStatus = listEntries.GroupBy(x => x.Status).ToDictionary(x => x.Key, x => x.ToArray());
            var everyValidStatus = Enum.GetValues(typeof(ListStatus)).Cast<ListStatus>().Except(new[] { ListStatus.Unknown });
            CollectionAssert.AreEquivalent(everyValidStatus, groupedByStatus.Keys);
            Assert.IsTrue(groupedByStatus.All(x => x.Value.Any()));
        }

        [Test, Ignore("Miner must be listening")]
        public void TestParseAnime()
        {
            var season = "2020/summer";
            Environment.SetEnvironmentVariable("seleniumMinerUrl", "http://localhost:22471/mine/");
            var testee = new SeleniumMiner();
            var animes = testee.MineSeasonAnime(season).Animes;
            CollectionAssert.IsNotEmpty(animes);
            System.IO.File.WriteAllText(
                "expected.json",
                JsonSerializer.Serialize(animes, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
        }
    }
}
