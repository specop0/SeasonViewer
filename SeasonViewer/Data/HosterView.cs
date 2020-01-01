using SeasonBackend.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeasonViewer.Data
{
    public class HosterView
    {
        public HosterView(Hoster model)
        {
            this.Model = model;
        }
        public Hoster Model { get; }
        public string Id => this.Model.Id;
        public string Name => this.Model.Name;

        public string HosterName => Enum.GetName(typeof(HosterType), this.Model.HosterType);

        public string HosterImageUrl
        {
            get
            {
                switch (this.Model.HosterType)
                {
                    case HosterType.Amazon:
                        return "icons/amazon.ico";
                    case HosterType.AnimeOnDemand:
                        return "icons/anime-on-demand.ico";
                    case HosterType.Crunchyroll:
                        return "icons/crunchyroll.ico";
                    case HosterType.Netflix:
                        return "icons/netflix.ico";
                    case HosterType.Wakanim:
                        return "icons/wakanim.ico";
                    default:
                        return "";
                }
            }
        }

        public string Url => this.Model.Url;
    }
}
