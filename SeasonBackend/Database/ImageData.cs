using LiteDB;

namespace SeasonBackend.Database;

public class ImageData
{
    [BsonId]
    public required string Id { get; set; }

    public required string MimeType { get; set; }
    public required byte[] Data { get; set; }
}
