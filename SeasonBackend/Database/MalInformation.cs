using System;
using System.Collections.Generic;
using System.Text;

namespace SeasonBackend.Database
{
    public class MalInformation
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string[] Names { get; set; }

        public string ImageUrl { get; set; }

        public ulong MemberCount { get; set; }

        public uint Score { get; set; }

        public ulong EpisodesCount { get; set; }
    }
}
