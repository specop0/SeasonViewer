using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SeasonBackend.Database;
using SeasonBackend.Miner;
using SeasonBackend.Protos;
using SeasonBackend.Services;

namespace SeasonBackend
{
    public class SeasonService : SeasonProvider.SeasonProviderBase
    {
        private readonly ILogger<SeasonService> _logger;
        public SeasonService(ILogger<SeasonService> logger)
        {
            _logger = logger;
        }

        public override Task<SeasonAnimeResponse> GetSeason(SeasonAnimeRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new SeasonAnimeResponse();

                var season = request.Name;

                var controller = ServicePool.Instance.GetService<DatabaseAccess>();
                var animes = controller.Do(x =>
                  {
                      return controller.GetSeasonAnimes(x, season).ToArray();
                  });

                if (!animes.Any())
                {
                    var miner = ServicePool.Instance.GetService<SeleniumMiner>();
                    var mineResult = miner.MineSeasonAnime(season);
                    if (mineResult.Animes.Any())
                    {
                        controller.Do(x =>
                        {
                            controller.InsertSeasonAnimes(x, mineResult.Animes);
                            animes = mineResult.Animes;
                        });
                    }
                }

                response.Animes.AddRange(animes.Select(this.Convert));

                return response;
            });
        }

        public SeasonAnime Convert(Anime anime)
        {
            var seasonAnime = new SeasonAnime
            {
                Id = anime.Id,
            };

            if(anime.Mal != null)
            {
                seasonAnime.MalId = anime.Mal.Id;
                seasonAnime.MalName = anime.Mal.Name ?? string.Empty; ;
                seasonAnime.MalImageUrl = anime.Mal.ImageUrl ?? string.Empty;
                seasonAnime.MalScore = anime.Mal.Score;
                seasonAnime.MalMembers = anime.Mal.MemberCount;
            }

            // TODO

            return seasonAnime;
        }
    }
}
