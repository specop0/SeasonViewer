using System.Text.Json.Serialization;

namespace SeasonViewer.Infrastructure.Miner;

public class ScreenshotRequest
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
