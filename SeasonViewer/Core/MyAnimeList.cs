namespace SeasonViewer.Core
{
    public class MyAnimeList
    {
        public required string Id { get; set; }

        public string Name { get; set; } = "";

        public string ImageUrl { get; set; } = "";

        public ulong MemberCount { get; set; }

        public uint Score { get; set; }

        public ulong EpisodesCount { get; set; }

        public ListStatus Status { get; set; }
    }
}
