using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using SeasonBackend.Database;
using SeasonBackend.Miner;
using SeasonBackend.Protos;

namespace SeasonBackend.Services
{
    public class SeasonService : SeasonProvider.SeasonProviderBase
    {
        public SeasonService(DatabaseAccess databaseAccess, SeleniumMiner miner, HosterService hosterService)
        {
            this.Controller = databaseAccess;
            this.Miner = miner;
            this.HosterService = hosterService;
        }

        protected DatabaseAccess Controller { get; }
        protected SeleniumMiner Miner { get; }
        protected HosterService HosterService { get; }

        public override Task<SeasonAnimeResponse> GetSeason(SeasonAnimeRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new SeasonAnimeResponse();

                var season = request.Name;

                var controller = this.Controller;
                var animes = controller.Do(x =>
                  {
                      return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria).ToArray();
                  });

                if (!animes.Any() && request.FilterCriteria == FilterCriteria.FilterByNone)
                {
                    var miner = this.Miner;
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

                var controller = this.Controller;
                var miner = this.Miner;

                IEnumerable<Anime> animes;
                if (Season.IsPlanToWatch(season))
                {
                    animes = controller.Do(x =>
                    {
                        return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                    });

                    var animesToUpdate = animes.Where(x => x.Mal.Name == "< UNKNOWN >").ToList();
                    if (!animesToUpdate.Any())
                    {
                        animesToUpdate = animes.ToList();
                    }

                    var mineResult = miner.MineAnimes(animesToUpdate);

                    animes = controller.Do(x =>
                    {
                        controller.InsertSeasonAnimes(x, mineResult);
                        return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                    });
                }
                else
                {
                    var mineResult = miner.MineSeasonAnime(season);
                    animes = controller.Do(x =>
                    {
                        controller.UpdateSeasonAnimes(x, season, mineResult.Animes);
                        return controller.GetSeasonAnimes(x, season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                    });
                }

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
                var isPlanToWatch = Season.IsPlanToWatch(season);

                var controller = this.Controller;
                var miner = this.Miner;
                // TODO get name via request and save it in database
                var mineResult = miner.MineMalList("specop0");
                var animes = controller.Do(x =>
                {
                    var animesDocument = controller.GetAnimeCollection(x);
                    var knownAnimeIds = new HashSet<long>();
                    var unknownAnimes = new List<Anime>();
                    foreach (var listEntry in mineResult)
                    {
                        var anime = animesDocument.FindOne(x => x.Mal.Id == listEntry.AnimeId);
                        if (anime != null)
                        {
                            anime.Mal.Status = listEntry.Status;
                            animesDocument.Update(anime);
                            knownAnimeIds.Add(anime.Id);
                        }
                        else
                        {
                            anime = new Anime
                            {
                                Seasons = new List<string>(),
                                Mal = new MalInformation
                                {
                                    Id = listEntry.AnimeId,
                                    Status = listEntry.Status,
                                    Name = "< UNKNOWN >",
                                }
                            };
                            unknownAnimes.Add(anime);
                        }
                    }

                    // animes with state "plan 2 watch" might be removed from the MAL list
                    if (isPlanToWatch)
                    {
                        var planToWatchAnimes = controller.GetSeasonAnimes(
                                x,
                                season,
                                OrderCriteria.OrderByNone,
                                GroupCriteria.GroupByNone,
                                FilterCriteria.FilterByPlan2Watch)
                            .ToList();

                        var missingAnimes = planToWatchAnimes
                            .Where(x => !knownAnimeIds.Contains(x.Id)) // .Except(knownAnimes)
                            .ToList();
                        var animesToDelete = new HashSet<long>();
                        foreach (var missingAnime in missingAnimes)
                        {
                            if (missingAnime.Mal.Status == ListStatus.Plan2Watch)
                            {
                                missingAnime.Mal.Status = ListStatus.Unknown;
                                animesDocument.Update(missingAnime);
                            }
                        }
                    }

                    if (unknownAnimes.Any())
                    {
                        controller.InsertSeasonAnimes(x, unknownAnimes);
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

                var controller = this.Controller;
                var anime = controller.Do(x => controller.GetAnime(x, id));

                var miner = this.Miner;
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

                var controller = this.Controller;

                var anime = controller.Do(x =>
                {
                    var anime = controller.GetAnime(x, id);

                    var miner = this.Miner;
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

            var hoster = anime.Hoster.Select(x =>
            {
                var hoster = new Hoster
                {
                    Name = x.Name ?? string.Empty,
                    HosterType = this.HosterService.GetHosterTypeFromUrl(x.Url),
                    Url = x.Url ?? string.Empty
                };

                return hoster;
            });
            seasonAnime.Hoster.AddRange(hoster);

            return seasonAnime;
        }
    }
}
