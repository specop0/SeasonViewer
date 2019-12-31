﻿using SeasonBackend.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeasonViewer.Data
{
    public class Anime
    {
        public Anime(SeasonAnime model)
        {
            this.Model = model;
        }

        public long Id => this.Model.Id;

        public string MalId => this.Model.MalId;

        public string Name => this.Model.MalName;

        public string ImageUrl => this.Model.MalImageUrl;

        public string MalUrl => $"https://myanimelist.net/anime/{this.MalId}";

        public uint MalScore => this.Model.MalScore;

        public ulong MalMembers => this.Model.MalMembers;

        public DateTime? HosterMinedAt => this.Model.HosterMinedAt > 0 ? new DateTime(this.Model.HosterMinedAt) : (DateTime?)null;

        public bool HosterMiningTriggered { get; set; }

        public string[] Names { get; set; }

        private SeasonAnime model;
        public SeasonAnime Model
        {
            get => this.model;
            set
            {
                this.model = value;

                this.Hoster.Clear();
                this.Hoster.AddRange(this.model.Hoster.Select(x => new HosterView(x)));
            }
        }

        public List<HosterView> Hoster { get; } = new List<HosterView>();
    }
}