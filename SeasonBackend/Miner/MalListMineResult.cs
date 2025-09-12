using SeasonBackend.Database;

namespace SeasonBackend.Miner
{
    public class MalListMineResult
    {
        public required string AnimeId { get; set; }

        public ListStatus Status { get; set; }
    }
}
