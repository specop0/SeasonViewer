using System;
using System.Collections.Generic;
using System.Text;

namespace SeasonBackend.Database
{
    public class Anime
    {
        public long Id { get; set; }

        public string Season { get; set; }

        public MalInformation Mal { get; set; }

        public HosterInformation[] Hoster { get; set; }
    }
}
