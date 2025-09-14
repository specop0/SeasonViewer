using System.Text.Json.Serialization;

namespace SeasonViewer.Infrastructure.Miner
{
    public class MinePageSourceResult
    {
        [JsonPropertyName("pageSource")]
        public string? PageSource { get; set; }
    }
}
