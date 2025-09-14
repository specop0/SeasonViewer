using SeasonViewer.UserInterface;

namespace SeasonViewer.Components.Shared
{
    public class HosterViewModel
    {
        public HosterViewModel(HosterDto model)
        {
            this.Model = model;
        }
        public HosterDto Model { get; }
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
