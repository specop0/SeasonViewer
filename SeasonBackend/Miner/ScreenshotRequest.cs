using System.Text.Json.Serialization;

namespace SeasonBackend.Miner;

public class ScreenshotRequest
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
}
