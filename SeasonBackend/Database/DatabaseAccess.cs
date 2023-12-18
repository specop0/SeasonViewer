using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Configuration;
using SeasonBackend.Protos;
using SeasonBackend.Services;

namespace SeasonBackend.Database
{
    public class DatabaseAccess : IDisposable
    {
        public DatabaseAccess(IConfiguration configuration, HosterService hosterService)
        {
            var databasePath = configuration.GetValue<string>("ConnectionStrings:FileDatabasePath");
            // https://www.litedb.org/
            this.Data = new LiteDatabase(databasePath);
            var animeCollection = this.GetAnimeCollection(this.Data);
            animeCollection.EnsureIndex(x => x.Seasons);
            animeCollection.EnsureIndex(x => x.Mal.Name);
            animeCollection.EnsureIndex(x => x.Mal.Id);

            this.HosterService = hosterService;
        }

        private static readonly object _lock = new();
        private LiteDatabase Data { get; }
        protected HosterService HosterService { get; }

        public void Do(Action<LiteDatabase> action)
        {
            lock (_lock)
            {
                action.Invoke(this.Data);
            }
        }
        public T Do<T>(Func<LiteDatabase, T> action)
        {
            lock (_lock)
            {
                return action.Invoke(this.Data);
            }
        }

        public IEnumerable<Anime> GetSeasonAnimes(LiteDatabase database, string season, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            IEnumerable<Anime> animes;
            if (Season.IsPlanToWatch(season))
            {
                animes = this.GetAnimeCollection(database).Find(x => x.Mal.Status == ListStatus.Plan2Watch);
            }
            else
            {
                animes = this.GetAnimeCollection(database).Find(x => x.Seasons.Contains(season));
            }

            // filter
            switch (filterBy)
            {
                case FilterCriteria.FilterByPlan2Watch:
                    animes = animes.Where(x => x.Mal.Status != ListStatus.Unknown && x.Mal.Status == ListStatus.Plan2Watch);
                    break;
            }

            // order
            switch (orderBy)
            {
                case OrderCriteria.OrderByScore:
                    animes = animes.OrderByDescending(x => x.Mal.Score);
                    break;
                case OrderCriteria.OrderByMember:
                    animes = animes.OrderByDescending(x => x.Mal.MemberCount);
                    break;
                case OrderCriteria.OrderByName:
                    animes = animes.OrderBy(x => x.Mal.Name);
                    break;
            }

            // group
            switch (groupBy)
            {
                case GroupCriteria.GroupByHoster:
                    animes = animes
                        .SelectMany(x =>
                        {
                            var hosterTypes = x.Hoster.Select(x => this.HosterService.GetHosterTypeFromUrl(x.Url)).Distinct().ToList();
                            if (!hosterTypes.Any())
                            {
                                hosterTypes.Add(string.Empty);
                            }

                            return hosterTypes.Select(y => Tuple.Create(x, y));
                        })
                        .GroupBy(x => x.Item2)
                        .OrderBy(x => x.Key, Comparer<string>.Create((x, y) =>
                        {
                            if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y)) return 0;

                            // null or empty should be last (i.e., highest)
                            // x is null? then y < x
                            if (string.IsNullOrEmpty(x)) return 1;
                            // y is null? then x < y
                            if (string.IsNullOrEmpty(y)) return -1;

                            return x.CompareTo(y);
                        }))
                        .SelectMany(x => x.Select(y => y.Item1));
                    break;
            }


            return animes;
        }

        public Anime GetAnime(LiteDatabase database, long id)
        {
            return this.GetAnimeCollection(database).Find(x => x.Id == id).FirstOrDefault();
        }

        public void UpdateHosters(LiteDatabase database, Anime anime, HosterInformation[] hosters)
        {
            anime.HosterMinedAt = DateTime.UtcNow;
            anime.Hoster = hosters.ToList();
            this.GetAnimeCollection(database).Update(anime);
        }

        public void InsertSeasonAnimes(LiteDatabase database, ICollection<Anime> animes)
        {
            var animeCollection = this.GetAnimeCollection(database);

            var animesToUpdate = new List<Anime>();
            var animesToAdd = new List<Anime>();
            foreach (var anime in animes)
            {
                var matchingAnime = animeCollection.FindOne(x => x.Mal.Id == anime.Mal.Id);
                if (matchingAnime != null)
                {
                    matchingAnime.Mal = anime.Mal;
                    var newSeasons = anime.Seasons.Except(matchingAnime.Seasons);
                    matchingAnime.Seasons.AddRange(newSeasons);
                    animesToUpdate.Add(matchingAnime);
                }
                else
                {
                    animesToAdd.Add(anime);
                }
            }

            animeCollection.Insert(animesToAdd);
            animeCollection.Update(animesToUpdate);
        }

        public ILiteCollection<Anime> GetAnimeCollection(LiteDatabase database)
        {
            return database.GetCollection<Anime>("animes");
        }

        public void UpdateSeasonAnimes(LiteDatabase database, string season, Anime[] animes)
        {
            var animeDocuments = this.GetAnimeCollection(database);
            foreach (var anime in animes)
            {
                var matchingAnime = animeDocuments.FindOne(x => x.Mal.Id == anime.Mal.Id);
                if (matchingAnime != null)
                {
                    // update only mal information
                    matchingAnime.Mal.EpisodesCount = anime.Mal.EpisodesCount;
                    matchingAnime.Mal.ImageUrl = anime.Mal.ImageUrl;
                    matchingAnime.Mal.MemberCount = anime.Mal.MemberCount;
                    matchingAnime.Mal.Name = anime.Mal.Name;
                    matchingAnime.Mal.Score = anime.Mal.Score;

                    // add season
                    if (!matchingAnime.Seasons.Contains(season))
                    {
                        matchingAnime.Seasons.Add(season);
                    }

                    animeDocuments.Update(matchingAnime);
                }
                else
                {
                    animeDocuments.Insert(anime);
                }
            }
        }

        public void Dispose()
        {
            this.Data?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
