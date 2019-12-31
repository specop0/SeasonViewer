using System.Text.Json.Serialization;

namespace SeasonBackend.Miner
{
    public class AmazonSearchRequest : MinePageSourceRequest
    {
        [JsonPropertyName("search")]
        public string Search { get; set; }
    }
}
