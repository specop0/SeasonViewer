using System;
using System.Collections.Generic;
using System.Linq;
using SeasonViewer.UserInterface;

namespace SeasonViewer.Components.Shared
{
    public class AnimeViewModel
    {
        public AnimeViewModel(AnimeDto model)
        {
            this.model = null!;
            this.Model = model;
        }

        public long Id => this.Model.Id;

        public string MalId => this.Model.MalId;

        public string Name => this.Model.MalName;

        public string ImageUrl => string.IsNullOrEmpty(this.Model.ImageId) ? "" : $"api/image/{this.Model.ImageId}";

        public string MalUrl => $"https://myanimelist.net/anime/{this.MalId}";

        public uint MalScore => this.Model.MalScore;

        public ulong MalMembers => this.Model.MalMembers;

        public ulong MalEpisodesCount => this.Model.MalEpisodesCount;

        public DateTime? HosterMinedAt => this.Model.HosterMinedAt > 0 ? new DateTime(this.Model.HosterMinedAt) : (DateTime?)null;

        public bool HosterMiningTriggered { get; set; }

        public bool AnimeMiningTriggered { get; set; }

        public string[] Names { get; set; } = [];

        private AnimeDto model;
        public AnimeDto Model
        {
            get => this.model;
            set
            {
                this.model = value;

                this.Hoster.Clear();
                this.Hoster.AddRange(this.model.Hoster.Select(x => new HosterViewModel(x)));

                this.HosterEdit.Clear();
            }
        }

        public List<HosterViewModel> Hoster { get; } = new List<HosterViewModel>();

        public bool HosterEditRequested { get; set; }

        public List<HosterDto> HosterEdit { get; } = new List<HosterDto>();

    }
}
