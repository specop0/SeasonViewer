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

        public string HosterImageUrl => this.Model.HosterType.ToImageUrl();

        public string Url => this.Model.Url;
    }
}
