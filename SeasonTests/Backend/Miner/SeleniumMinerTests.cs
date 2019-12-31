using NUnit.Framework;
using SeasonBackend.Database;
using SeasonBackend.Miner;
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
