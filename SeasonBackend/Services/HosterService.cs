using Microsoft.Extensions.Configuration;

namespace SeasonBackend.Services;

public class HosterService
{
    public HosterService(IConfiguration configuration)
    {
        this.mappings = configuration.GetSection("HosterMapping").Get<HosterMappingOption[]>() ?? [];
    }

    private readonly HosterMappingOption[] mappings;

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
