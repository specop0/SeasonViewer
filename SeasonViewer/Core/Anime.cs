using System;
using System.Collections.Generic;
using LiteDB;

namespace SeasonViewer.Core
{
    public class Anime
    {
        [BsonId]
        public long Id { get; set; }

        public List<string> Seasons { get; set; } = [];

        public required MyAnimeList Mal { get; set; }

        public List<Hoster> Hoster { get; set; } = [];

        public DateTime? HosterMinedAt { get; set; }
    }
}
