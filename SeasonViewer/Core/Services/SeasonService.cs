using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using SeasonViewer.Core.Hosters;
using SeasonViewer.Core.Miner;

namespace SeasonViewer.Core.Services
{
    public class SeasonService
    {
        public SeasonService(IDatabaseService databaseService, ISeleniumMiner miner, IHosterService hosterService)
        {
            this.DatabaseService = databaseService;
            this.Miner = miner;
            this.HosterService = hosterService;
        }

        protected IDatabaseService DatabaseService { get; }
        protected ISeleniumMiner Miner { get; }
        protected IHosterService HosterService { get; }

        public async Task<ICollection<Anime>> GetSeasonAsync(SeasonAnimeRequest request)
        {
            var season = request.Name;

            ICollection<Anime> animes = this.DatabaseService.Do(context =>
            {
                return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria).ToArray();
            });

            if (!animes.Any() && request.FilterCriteria == FilterCriteria.FilterByNone)
            {
                var miner = this.Miner;
                var minedAnimes = await miner.MineSeasonAnimeAsync(season);
                if (minedAnimes.Any())
                {
                    this.DatabaseService.Do(context =>
                    {
                        context.InsertSeasonAnimes(minedAnimes);
                        animes = minedAnimes;
                    });
                }
            }

            return animes;
        }

        public async Task<ICollection<Anime>> UpdateSeasonAsync(SeasonAnimeRequest request)
        {
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
                    animesToUpdate = [.. animes];
                }

                var minedAnimes = await miner.MineAnimesAsync(animesToUpdate);

                animes = this.DatabaseService.Do(context =>
                {
                    context.InsertSeasonAnimes(minedAnimes);
                    return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });
            }
            else
            {
                var minedAnimes = await miner.MineSeasonAnimeAsync(season);
                animes = this.DatabaseService.Do(context =>
                {
                    context.UpdateSeasonAnimes(season, minedAnimes);
                    return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
                });
            }

            return [.. animes];
        }

        public async Task<ICollection<Anime>> UpdateMalListAsync(SeasonAnimeRequest request)
        {
            var season = request.Name;
            var isPlanToWatch = Season.IsPlanToWatch(season);

            var miner = this.Miner;
            // TODO get name via request and save it in database
            var mineResult = await miner.MineMalListAsync("specop0");
            var animes = this.DatabaseService.Do(context =>
            {
                var knownAnimeIds = new HashSet<long>();
                var unknownAnimes = new List<Anime>();
                foreach (var listEntry in mineResult)
                {
                    var anime = context.GetAnimeByMalId(listEntry.AnimeId);
                    if (anime != null)
                    {
                        anime.Mal.Status = listEntry.Status;
                        context.UpdateAnime(anime);
                        knownAnimeIds.Add(anime.Id);
                    }
                    else
                    {
                        anime = new Anime
                        {
                            Seasons = new List<string>(),
                            Mal = new MyAnimeList
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
                            FilterCriteria.FilterByPlan2Watch);

                    var missingAnimes = planToWatchAnimes
                        .Where(x => !knownAnimeIds.Contains(x.Id)); // .Except(knownAnimes)
                    var animesToDelete = new HashSet<long>();
                    foreach (var missingAnime in missingAnimes)
                    {
                        if (missingAnime.Mal.Status == ListStatus.Plan2Watch)
                        {
                            missingAnime.Mal.Status = ListStatus.Unknown;
                            context.UpdateAnime(missingAnime);
                        }
                    }
                }

                if (unknownAnimes.Any())
                {
                    context.InsertSeasonAnimes(unknownAnimes);
                }

                return context.GetSeasonAnimes(season, request.OrderCriteria, request.GroupCriteria, request.FilterCriteria);
            });

            return [.. animes];
        }

        public async Task<Anime> MineMalAsync(long id)
        {
            var anime = this.DatabaseService.Do(context => context.GetAnime(id));

            var newMalInformation = await this.Miner.MineMalAsync(anime);
            if (newMalInformation != null)
            {
                anime = this.DatabaseService.Do(context =>
                {
                    anime.Mal = newMalInformation;
                    context.UpdateAnime(anime);
                    return context.GetAnime(id);
                });
            }

            return anime;
        }

        public async Task<Anime> MineHosterAsync(long id)
        {
            var anime = this.DatabaseService.Do(context => context.GetAnime(id));

            var miner = this.Miner;
            var minedHosters = await miner.MineHosterAsync(anime);

            anime = this.DatabaseService.Do(context =>
            {
                anime.HosterMinedAt = DateTime.UtcNow;
                anime.Hoster = [.. minedHosters];
                context.UpdateAnime(anime);
                return context.GetAnime(id);
            });

            return anime;
        }

        public Task<Anime> EditHosterAsync(long id, ICollection<Hoster> hosters)
        {
            return Task.Run(() =>
            {
                var anime = this.DatabaseService.Do(context =>
                {
                    var anime = context.GetAnime(id);

                    foreach (var hoster in hosters)
                    {
                        if (string.IsNullOrEmpty(hoster.Name))
                        {
                            hoster.Name = anime.Mal.Name;
                        }
                    }

                    anime.HosterMinedAt = DateTime.UtcNow;
                    anime.Hoster = [.. hosters];
                    context.UpdateAnime(anime);

                    return anime;
                });

                return anime;
            });
        }

        public async Task<ImageData> GetImageDataAsync(string id)
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
                        Data = [],
                        MimeType = string.Empty,
                    };
                }

                this.DatabaseService.Do(context => context.SetImageData(imageData));
            }

            return imageData;
        }
    }
}