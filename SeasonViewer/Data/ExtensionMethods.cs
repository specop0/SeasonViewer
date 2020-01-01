using SeasonBackend.Protos;

namespace SeasonViewer.Data
{
    public static class ExtensionMethods
    {
        public static string ToImageUrl(this HosterType type)
        {
            switch (type)
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
}
