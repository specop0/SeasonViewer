using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.StaticFiles;
using SeasonBackend.Database;
using SeasonBackend.Miner;
using SeasonBackend.Protos;

namespace SeasonBackend.Services
{
    public class SeasonService : SeasonProvider.SeasonProviderBase
    {
        public SeasonService(DatabaseService databaseService, SeleniumMiner miner, HosterService hosterService)
        {
            this.DatabaseService = databaseService;
            this.Miner = miner;
            this.HosterService = hosterService;
        }

        protected DatabaseService DatabaseService { get; }
        protected SeleniumMiner Miner { get; }
        protected HosterService HosterService { get; }

        public async override Task<SeasonAnimeResponse> GetSeason(SeasonAnimeRequest request, ServerCallContext context)
        {
            var response = new SeasonAnimeResponse();

            var season = request.Name;

            var animes = this.DatabaseService.Do(context =>
              {
                  return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria).ToArray();
              });

            if (!animes.Any() && request.FilterCriteria == FilterCriteria.FilterByNone)
            {
                var miner = this.Miner;
                var mineResult = await miner.MineSeasonAnimeAsync(season);
                if (mineResult.Animes.Any())
                {
                    this.DatabaseService.Do(context =>
                    {
                        context.InsertSeasonAnimes(mineResult.Animes);
                        animes = mineResult.Animes;
                    });
                }
            }

            response.Animes.AddRange(animes.Select(this.Convert));

            return response;
        }

        public async override Task<SeasonAnimeResponse> UpdateSeason(SeasonAnimeRequest request, ServerCallContext context)
        {
            var response = new SeasonAnimeResponse();

            var season = request.Name;

            var miner = this.Miner;

            IEnumerable<Anime> animes;
            if (Season.IsPlanToWatch(season))
            {
                animes = this.DatabaseService.Do(context =>
                {
                    return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });

                var animesToUpdate = animes.Where(x => x.Mal.Name == "< UNKNOWN >").ToList();
                if (!animesToUpdate.Any())
                {
                    animesToUpdate = animes.ToList();
                }

                var mineResult = await miner.MineAnimesAsync(animesToUpdate);

                animes = this.DatabaseService.Do(context =>
                {
                    context.InsertSeasonAnimes(mineResult);
                    return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });
            }
            else
            {
                var mineResult = await miner.MineSeasonAnimeAsync(season);
                animes = this.DatabaseService.Do(context =>
                {
                    context.UpdateSeasonAnimes(season, mineResult.Animes);
                    return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });
            }

            response.Animes.AddRange(animes.Select(this.Convert));

            return response;
        }

        public async override Task<SeasonAnimeResponse> UpdateMalList(SeasonAnimeRequest request, ServerCallContext context)
        {
            var response = new SeasonAnimeResponse();

            var season = request.Name;
            var isPlanToWatch = Season.IsPlanToWatch(season);

            var miner = this.Miner;
            // TODO get name via request and save it in database
            var mineResult = await miner.MineMalListAsync("specop0");
            var animes = this.DatabaseService.Do(context =>
            {
                var animesDocument = context.GetAnimeCollection();
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
                    var planToWatchAnimes = context.GetSeasonAnimes(
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
                    context.InsertSeasonAnimes(unknownAnimes);
                }

                return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
            });

            response.Animes.AddRange(animes.Select(this.Convert));

            return response;
        }

        public override async Task<MineAnimeResponse> MineMal(MineAnimeRequest request, ServerCallContext context)
        {
            var response = new MineAnimeResponse();

            var anime = this.DatabaseService.Do(context => context.GetAnime(request.Id));

            var newMalInformation = await this.Miner.MineMalAsync(anime);
            if (newMalInformation != null)
            {
                anime = this.DatabaseService.Do(context =>
                {
                    context.UpdateMal(anime, newMalInformation);
                    return context.GetAnime(request.Id);
                });
            }

            response.Anime = this.Convert(anime);
            return response;
        }

        public async override Task<MineHosterResponse> MineHoster(MineHosterRequest request, ServerCallContext context)
        {
            var response = new MineHosterResponse();

            var id = request.Id;

            var anime = this.DatabaseService.Do(context => context.GetAnime(id));

            var miner = this.Miner;
            var mineResult = await miner.MineHosterAsync(anime);

            anime = this.DatabaseService.Do(context =>
            {
                context.UpdateHosters(anime, mineResult.Hosters);
                return context.GetAnime(request.Id);
            });

            response.Anime = this.Convert(anime);
            return response;
        }

        public override Task<EditHosterResponse> EditHoster(EditHosterRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new EditHosterResponse();

                var id = request.Id;

                var anime = this.DatabaseService.Do(context =>
                {
                    var anime = context.GetAnime(id);

                    var miner = this.Miner;
                    var hosters = miner.ParseHoster(anime, request.Hosters);

                    context.UpdateHosters(anime, hosters);

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
                seasonAnime.ImageId = string.Empty;
                if (!string.IsNullOrEmpty(anime.Mal.ImageUrl))
                {
                    seasonAnime.ImageId = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(anime.Mal.ImageUrl));
                }
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

        public override async Task<ImageDataResponse> GetImageData(ImageDataRequest request, ServerCallContext context)
        {
            var response = new ImageDataResponse();

            if (string.IsNullOrEmpty(request.Id))
            {
                return response;
            }

            var imageData = await this.GetImageData(request.Id);

            response.MimeType = imageData.MimeType ?? string.Empty;
            if (imageData.Data != null)
            {
                response.Data = ByteString.CopyFrom(imageData.Data);
            }

            return response;
        }

        private async Task<ImageData> GetImageData(string id)
        {
            var imageData = this.DatabaseService.Do(context => context.GetImageData(id));

            if (imageData == null)
            {
                var imageUrl = Encoding.UTF8.GetString(System.Convert.FromBase64String(id));

                var data = await this.Miner.MineImageAsync(imageUrl);

                if (data.Any())
                {
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(imageUrl, out var mimeType))
                    {
                        mimeType = "application/octet-stream";
                    }

                    imageData = new ImageData
                    {
                        Id = id,
                        Data = data,
                        MimeType = mimeType,
                    };
                }
                else
                {
                    imageData = new ImageData
                    {
                        Id = id,
                        MimeType = string.Empty,
                    };
                }

                this.DatabaseService.Do(context => context.SetImageData(imageData));
            }

            return imageData;
        }
    }
}
