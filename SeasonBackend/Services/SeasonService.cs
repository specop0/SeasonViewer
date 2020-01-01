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
                      return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria).ToArray();
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

        public override Task<SeasonAnimeResponse> UpdateSeason(SeasonAnimeRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new SeasonAnimeResponse();

                var season = request.Name;

                var controller = ServicePool.Instance.GetService<DatabaseAccess>();
                var miner = ServicePool.Instance.GetService<SeleniumMiner>();
                var mineResult = miner.MineSeasonAnime(season);
                var animes = controller.Do(x =>
                {
                    controller.UpdateSeasonAnimes(x, mineResult.Animes);
                    return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });

                response.Animes.AddRange(animes.Select(this.Convert));

                return response;
            });
        }

        public override Task<SeasonAnimeResponse> UpdateMalList(SeasonAnimeRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new SeasonAnimeResponse();

                var season = request.Name;

                var controller = ServicePool.Instance.GetService<DatabaseAccess>();
                var miner = ServicePool.Instance.GetService<SeleniumMiner>();
                // TODO get name via request and save it better in database
                var mineResult = miner.MineMalList("specop0");
                var animes = controller.Do(x =>
                {
                    var animesDocument = controller.GetAnimeCollection(x);
                    foreach (var listEntry in mineResult)
                    {
                        var anime = animesDocument.FindOne(x => x.Mal.Id == listEntry.AnimeId);
                        if (anime != null)
                        {
                            anime.Mal.Status = listEntry.Status;
                            animesDocument.Update(anime);
                        }
                    }

                    return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });

                response.Animes.AddRange(animes.Select(this.Convert));

                return response;
            });
        }

        public override Task<MineHosterResponse> MineHoster(MineHosterRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new MineHosterResponse();

                var id = request.Id;

                var controller = ServicePool.Instance.GetService<DatabaseAccess>();
                var anime = controller.Do(x => controller.GetAnime(x, id));

                var miner = ServicePool.Instance.GetService<SeleniumMiner>();
                var mineResult = miner.MineHoster(anime);

                controller.Do(x =>
                {
                    controller.UpdateHosters(x, anime, mineResult.Hosters);
                });

                response.Anime = this.Convert(anime);
                return response;
            });
        }

        public override Task<EditHosterResponse> EditHoster(EditHosterRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new EditHosterResponse();

                var id = request.Id;

                var controller = ServicePool.Instance.GetService<DatabaseAccess>();

                var anime = controller.Do(x =>
                {
                    var anime = controller.GetAnime(x, id);

                    var miner = ServicePool.Instance.GetService<SeleniumMiner>();
                    var hosters = miner.ParseHoster(anime, request.Hosters);

                    controller.UpdateHosters(x, anime, hosters);

                    return anime;
                });

                response.Anime = this.Convert(anime);
                return response;
            });
        }

        public SeasonAnime Convert(Anime anime)
        {
            var seasonAnime = new SeasonAnime
            {
                Id = anime.Id,
            };

            if (anime.Mal != null)
            {
                seasonAnime.MalId = anime.Mal.Id;
                seasonAnime.MalName = anime.Mal.Name ?? string.Empty; ;
                seasonAnime.MalImageUrl = anime.Mal.ImageUrl ?? string.Empty;
                seasonAnime.MalScore = anime.Mal.Score;
                seasonAnime.MalMembers = anime.Mal.MemberCount;
                seasonAnime.MalEpisodesCount = anime.Mal.EpisodesCount;
            }

            if (anime.HosterMinedAt.HasValue)
            {
                seasonAnime.HosterMinedAt = anime.HosterMinedAt.Value.Ticks;
            }

            if (anime.Hoster != null)
            {
                var hoster = anime.Hoster.Select(x =>
                {
                    var hoster = new Hoster();

                    hoster.Id = x.Id ?? string.Empty;
                    hoster.Name = x.Name ?? string.Empty;
                    hoster.HosterType = x.HosterType;
                    hoster.Url = x.Url ?? string.Empty;

                    return hoster;
                });
                seasonAnime.Hoster.AddRange(hoster);
            }

            return seasonAnime;
        }
    }
}
