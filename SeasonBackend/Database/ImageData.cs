using LiteDB;

namespace SeasonBackend.Database;

public class ImageData
{
    [BsonId]
    public string Id { get; set; }

    public string MimeType { get; set; }
    public byte[] Data { get; set; }
}
