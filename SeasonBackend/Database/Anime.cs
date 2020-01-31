using System;
using System.Collections.Generic;
using System.Text;

namespace SeasonBackend.Database
{
    public class Anime
    {
        public long Id { get; set; }

        public string[] Seasons { get; set; }

        public MalInformation Mal { get; set; }

        public HosterInformation[] Hoster { get; set; }

        public DateTime? HosterMinedAt { get; set; }
    }
}
