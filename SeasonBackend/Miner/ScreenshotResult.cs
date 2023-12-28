using System.Text.Json.Serialization;

namespace SeasonBackend.Miner;

public class ScreenshotResult
{
    [JsonPropertyName("imageData")]
    public string ImageData { get; set; }
}
