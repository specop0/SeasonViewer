using SeasonBackend.Services;

namespace SeasonBackend.Database
{
    public class HosterInformation
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string HosterType
        {
            get
            {
                if (string.IsNullOrEmpty(this.Url))
                {
                    return string.Empty;
                }

                return ServicePool.Instance.GetService<HosterService>().GetHosterTypeFromUrl(this.Url);
            }
        }
    }
}
