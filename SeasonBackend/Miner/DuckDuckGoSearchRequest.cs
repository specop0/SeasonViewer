using System.Text.Json.Serialization;

namespace SeasonBackend.Miner
{
    public class DuckDuckGoSearchRequest
    {
        [JsonPropertyName("search")]
        public string Search { get; set; }
    }
}
