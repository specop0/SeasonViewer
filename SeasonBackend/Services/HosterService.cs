using Microsoft.Extensions.Configuration;

namespace SeasonBackend.Services;

public class HosterService
{
    private HosterMappingOption[] mappings = new HosterMappingOption[0];

    public void Initialize(IConfiguration configuration)
    {
        this.mappings = configuration.GetSection("HosterMapping").Get<HosterMappingOption[]>();
    }

    public string GetHosterTypeFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        foreach (var mapping in this.mappings)
        {
            if (url.StartsWith(mapping.Pattern))
            {
                return mapping.Key;
            }
        }

        return string.Empty;
    }
}
