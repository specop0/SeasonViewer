using System.Text.Json.Serialization;

namespace SeasonBackend.Miner
{
    public class MinePageSourceResult
    {
        [JsonPropertyName("pageSource")]
        public string? PageSource { get; set; }
    }
}
