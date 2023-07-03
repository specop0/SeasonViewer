using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using SeasonBackend.Protos;
using static SeasonBackend.Protos.SeasonProvider;

namespace SeasonViewer.Data
{
    public class AnimeSeasonService
    {
        public AnimeSeasonService(IConfiguration configuration)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var options = new GrpcChannelOptions();
            var backendUrl = configuration.GetValue<string>("BackendUrl");
            var channel = GrpcChannel.ForAddress(backendUrl, options);

            this.Client = new SeasonProviderClient(channel);
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
    }
}
