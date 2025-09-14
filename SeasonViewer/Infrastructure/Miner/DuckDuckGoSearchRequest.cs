using System.Text.Json.Serialization;

namespace SeasonViewer.Infrastructure.Miner
{
    public class DuckDuckGoSearchRequest
    {
        [JsonPropertyName("search")]
        public string? Search { get; set; }
    }
}
