using NUnit.Framework;
using SeasonBackend.Database;
using SeasonBackend.Miner;
using SeasonBackend.Protos;
using System.Linq;
using System.Text.Json;

namespace SeasonTests.Backend.Miner
{
    class SeleniumMinerTests : TestBase
    {
        [Test]
        public void TestParseAnimeFromJson()
        {
            var input = TestResources.mal_season;
            var season = this.GetUniqueName();
            var actualAnimes = SeleniumMiner.ParseSeasonAnime(input, season);
            CollectionAssert.IsNotEmpty(actualAnimes);

            var expectedAnimes = JsonSerializer.Deserialize<Anime[]>(TestResources.mal_season_expected);
            CollectionAssert.IsNotEmpty(expectedAnimes, "unit test implementation");
            //System.IO.File.WriteAllText(@"C:\Users\SpecOp0\Desktop\mal-season-expected.json", JsonSerializer.Serialize(actualAnimes, new JsonSerializerOptions { IgnoreNullValues = true }));

            Assert.AreEqual(expectedAnimes.Length, actualAnimes.Length);

            for (var i = 0; i < expectedAnimes.Length; i++)
            {
                var expected = expectedAnimes[i];
                var actual = actualAnimes[i];

                Assert.AreEqual(expected.Mal.Id, actual.Mal.Id);
                Assert.AreEqual(expected.Mal.Name, actual.Mal.Name);
                Assert.IsNull(actual.Mal.Names);
                Assert.AreEqual(expected.Mal.ImageUrl, actual.Mal.ImageUrl);
                Assert.AreEqual(expected.Mal.MemberCount, actual.Mal.MemberCount);
                Assert.AreEqual(expected.Mal.Score, actual.Mal.Score);

                Assert.AreEqual(season, actual.Season);
                Assert.AreEqual(default(int), actual.Id);

                Assert.IsNull(actual.Hoster);
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
