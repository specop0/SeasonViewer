using System.Text.Json.Serialization;

namespace SeasonBackend.Miner
{
    public class MinePageSourceRequest
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
