﻿using System;
using System.Collections.Generic;

namespace SeasonBackend.Database
{
    public class Anime
    {
        public long Id { get; set; }

        public List<string> Seasons { get; set; } = new List<string>();

        public MalInformation Mal { get; set; }

        public List<HosterInformation> Hoster { get; set; } = new List<HosterInformation>();

        public DateTime? HosterMinedAt { get; set; }
    }
}
