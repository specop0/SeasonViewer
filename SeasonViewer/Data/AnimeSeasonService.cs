using System.Linq;
using System.Threading.Tasks;
using SeasonBackend.Protos;
using static SeasonBackend.Protos.SeasonProvider;

namespace SeasonViewer.Data
{
    public class AnimeSeasonService
    {
        public AnimeSeasonService(SeasonProviderClient client)
        {
            this.Client = client;
        }

        private SeasonProviderClient Client { get; }

        public async Task<Anime[]> GetSeasonAsync(string request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = new SeasonAnimeRequest();
            seasonAnimeRequest.Name = request;
            seasonAnimeRequest.OrderCriteria = orderBy;
            seasonAnimeRequest.GroupCriteria = groupBy;
            seasonAnimeRequest.FilterCriteria = filterBy;
            var response = await this.Client.GetSeasonAsync(seasonAnimeRequest);
            return response.Animes.Select(x => new Anime(x)).ToArray();
        }

        public async Task<Anime[]> UpdateSeasonAsync(string request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = new SeasonAnimeRequest();
            seasonAnimeRequest.Name = request;
            seasonAnimeRequest.OrderCriteria = orderBy;
            seasonAnimeRequest.GroupCriteria = groupBy;
            seasonAnimeRequest.FilterCriteria = filterBy;
            var response = await this.Client.UpdateSeasonAsync(seasonAnimeRequest);
            return response.Animes.Select(x => new Anime(x)).ToArray();
        }

        public async Task<Anime[]> UpdateMalListAsync(string request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = new SeasonAnimeRequest();
            seasonAnimeRequest.Name = request;
            seasonAnimeRequest.OrderCriteria = orderBy;
            seasonAnimeRequest.GroupCriteria = groupBy;
            seasonAnimeRequest.FilterCriteria = filterBy;
            var response = await this.Client.UpdateMalListAsync(seasonAnimeRequest);
            return response.Animes.Select(x => new Anime(x)).ToArray();
        }

        public async Task<MineAnimeResponse> MineMalAsync(Anime anime)
        {
            var request = new MineAnimeRequest
            {
                Id = anime.Id,
            };
            var response = await this.Client.MineMalAsync(request);
            return response;
        }

        public async Task<MineHosterResponse> MineHosterAsync(Anime anime)
        {
            var mineHosterRequest = new MineHosterRequest
            {
                Id = anime.Id
            };
            var response = await this.Client.MineHosterAsync(mineHosterRequest);
            return response;
        }

        public async Task<EditHosterResponse> EditHosterAsync(Anime anime)
        {
            var editHosterRequest = new EditHosterRequest
            {
                Id = anime.Id,
            };
            editHosterRequest.Hosters.AddRange(anime.HosterEdit);

            var response = await this.Client.EditHosterAsync(editHosterRequest);

            return response;
        }

        public async Task<ImageDataResponse> GetImageDataAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new ImageDataResponse();
            }

            var request = new ImageDataRequest
            {
                Id = id,
            };

            var response = await this.Client.GetImageDataAsync(request);

            return response;
        }
    }
}
