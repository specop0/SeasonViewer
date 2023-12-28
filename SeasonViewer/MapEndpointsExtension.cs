using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SeasonViewer.Data;

namespace SeasonViewer;

public static class MapEndpointsExtension
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/api/image/{id}", async (string id, AnimeSeasonService service) =>
        {
            var image = await service.GetImageDataAsync(id);

            if (string.IsNullOrEmpty(image?.MimeType) || (image?.Data?.IsEmpty ?? true))
            {
                return Results.NoContent();
            }

            return Results.File(image.Data.ToByteArray(), image.MimeType);
        });
    }
}