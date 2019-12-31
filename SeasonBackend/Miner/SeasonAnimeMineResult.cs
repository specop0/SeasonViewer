using SeasonBackend.Database;

namespace SeasonBackend.Miner
{
    public class SeasonAnimeMineResult
    {
        public string PageSource { get; set; }

        public Anime[] Animes { get; set; } = new Anime[0];
    }
}
