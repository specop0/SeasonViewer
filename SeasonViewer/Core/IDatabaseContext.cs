using System.Collections.Generic;
using SeasonViewer.Core.Services;

namespace SeasonViewer.Core
{
    public interface IDatabaseContext
    {
        Anime GetAnime(long id);
        Anime? GetAnimeByMalId(string malId);
        IEnumerable<Anime> GetSeasonAnimes(string season, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy);
        void UpdateAnime(Anime anime);
        ImageData GetImageData(string id);
        void SetImageData(ImageData imageData);
        void InsertSeasonAnimes(ICollection<Anime> animes);
        void UpdateSeasonAnimes(string season, ICollection<Anime> animes);
    }
}
