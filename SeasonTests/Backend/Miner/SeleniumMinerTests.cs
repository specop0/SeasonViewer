using NUnit.Framework;
using SeasonBackend.Database;
using SeasonBackend.Miner;
using SeasonBackend.Protos;
using System;
using System.Linq;
using System.Text.Json;

namespace SeasonTests.Backend.Miner
{
    class SeleniumMinerTests : TestBase
    {
        [Test]
        public void TestParseSingleAnimeFromJson()
        {
            var input = TestResources.anime;
            var actualAnime = SeleniumMiner.ParseAnime(input);

            Assert.IsNotNull(actualAnime);
            Assert.AreEqual("Boku no Hero Academia 4th Season", actualAnime.Mal.Name);
            Assert.AreEqual("38408", actualAnime.Mal.Id);
            Assert.AreEqual(25, actualAnime.Mal.EpisodesCount);
            Assert.AreEqual(826, actualAnime.Mal.Score);
            Assert.AreEqual(544810, actualAnime.Mal.MemberCount);
            Assert.AreEqual("https://cdn.myanimelist.net/images/anime/1315/102961.jpg", actualAnime.Mal.ImageUrl);
        }

        [Test]
        public void TestParseAnimeFromJson()
        {
            var input = TestResources.mal_season;
            var season = this.GetUniqueName();
            var actualAnimes = SeleniumMiner.ParseSeasonAnime(input, season);
            CollectionAssert.IsNotEmpty(actualAnimes);

            var expectedAnimes = JsonSerializer.Deserialize<Anime[]>(TestResources.mal_season_expected);
            CollectionAssert.IsNotEmpty(expectedAnimes, "unit test implementation");
            //System.IO.File.WriteAllText(@"C:\Users\SpecOp0\Desktop\mal-season-expected.json", JsonSerializer.Serialize(actualAnimes, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = true }));

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

                Assert.IsNull(actual.Hoster);

                Assert.IsNotNull(expected.Mal.ImageUrl, $"missing image for {expected.Mal.Name}");
            }
        }

        [Test]
        public void TestParseAmazonFromJson([Values(true, false)] bool exactMatch)
        {
            var input = TestResources.amazon_search;
            var anime = new Anime { Mal = new MalInformation { Name = exactMatch ? "Psycho-Pass 3" : "Psycho-Pass" } };
            var hosterInformations = SeleniumMiner.ParseAmazonSearch(anime, input);

            if (exactMatch)
            {
                Assert.AreEqual(1, hosterInformations.Length);
                var hosterInformation = hosterInformations.Single();
                Assert.AreEqual("PSYCHO-PASS 3", hosterInformation.Name);
                Assert.AreEqual("B07ZHQ34WW", hosterInformation.Id);
                Assert.AreEqual(HosterType.Amazon, hosterInformation.HosterType);
            }
            else
            {
                var expectedHosterInformations = JsonSerializer.Deserialize<HosterInformation[]>(TestResources.amazon_search_expected);
                CollectionAssert.IsNotEmpty(expectedHosterInformations, "unit test implementation");

                Assert.AreEqual(expectedHosterInformations.Length, hosterInformations.Length);

                for (var i = 0; i < expectedHosterInformations.Length; i++)
                {
                    var expected = expectedHosterInformations[i];
                    var actual = hosterInformations[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Name, actual.Name);
                    Assert.AreEqual(expected.HosterType, actual.HosterType);
                }
            }
        }

        [Test]
        public void TestParseDuckDuckGoSearchFromJson()
        {
            var input = TestResources.duckduckgo_search;
            var searchResult = SeleniumMiner.ParseDuckDuckGoSearch(input);

            var expectedResults = JsonSerializer.Deserialize<DuckDuckGoSearchItem[]>(TestResources.duckduckgo_search_expected);
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
            var input = TestResources.mal_list;
            var listEntries = SeleniumMiner.ParseMalList(input);

            Assert.Greater(listEntries.Count(), 1000);

            var expectedResults = JsonSerializer.Deserialize<MalListMineResult[]>(TestResources.mal_list_expected);
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
            var season = "2019/fall";
            var testee = new SeleniumMiner();
            var animes = testee.MineSeasonAnime(season).Animes;
            CollectionAssert.IsNotEmpty(animes);
        }
    }
}
