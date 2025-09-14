using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SeasonViewer.Core.Services;

namespace SeasonViewer;

public static class MapEndpointsExtension
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/api/image/{id}", async (string id, SeasonService service) =>
        {
            var image = await service.GetImageDataAsync(id);

            if (string.IsNullOrEmpty(image.MimeType) || (image.Data.Length == 0))
            {
                return Results.NoContent();
            }

            return Results.File(image.Data, image.MimeType);
        });
    }
}