using System.Text.Json.Serialization;

namespace SeasonViewer.Infrastructure.Miner;

public class ScreenshotResult
{
    [JsonPropertyName("imageData")]
    public string? ImageData { get; set; }
}
