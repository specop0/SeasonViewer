using Grpc.Net.Client;
using SeasonBackend.Protos;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SeasonBackend.Protos.SeasonProvider;

namespace SeasonViewer.Data
{
    public class AnimeSeasonService
    {
        public AnimeSeasonService()
        {
            var options = new GrpcChannelOptions();
            var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("seasonBackendUrl"), options);

            this.Client = new SeasonProviderClient(channel);
        }

        private SeasonProviderClient Client { get; }

        public async Task<Anime[]> GetSeasonAsync()
        {
            var seasonAnimeRequest = new SeasonAnimeRequest();
            seasonAnimeRequest.Name = "2019/fall";
            var response = await this.Client.GetSeasonAsync(seasonAnimeRequest);
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
    }
}
