using SeasonBackend.Database;

namespace SeasonBackend.Miner
{
    public class MineHosterResult
    {
        public string PageSource { get; set; }

        public HosterInformation[] Hosters { get; set; } = new HosterInformation[0];
    }
}
