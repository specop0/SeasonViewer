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

        public string Url
        {
            get
            {
                switch (this.Model.HosterType)
                {
                    case HosterType.Amazon:
                        return $"https://www.amazon.de/dp/{this.Id}";
                    default:
                        return null;
                }
            }
        }

    }
}
