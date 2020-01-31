using System;
using System.Collections.Generic;
using System.Text;

namespace SeasonBackend.Database
{
    public class Anime
    {
        public long Id { get; set; }

        public List<string> Seasons { get; set; }

        public MalInformation Mal { get; set; }

        public List<HosterInformation> Hoster { get; set; }

        public DateTime? HosterMinedAt { get; set; }
    }
}
