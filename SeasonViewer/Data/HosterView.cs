using SeasonBackend.Protos;

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

        public string HosterName => string.IsNullOrEmpty(this.Model.HosterType)
            ? "UNKNOWN"
            : this.Model.HosterType;

        public string HosterImageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(this.Model.HosterType))
                {
                    return "";
                }
                return $"icons/{this.Model.HosterType}.ico";
            }
        }

        public string Url => this.Model.Url;
    }
}
