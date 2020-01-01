using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeasonBackend.Database
{
    public class DatabaseAccess : IDisposable
    {
        public DatabaseAccess(string databaseName)
        {
            // https://www.litedb.org/
            this.Data = new LiteDatabase(databaseName);
            this.Data.GetCollection<Anime>("animes").EnsureIndex(x => x.Season);
            this.Data.GetCollection<Anime>("animes").EnsureIndex(x => x.Mal.Name);
        }

        private LiteDatabase Data { get; }

        public void Do(Action<LiteDatabase> action)
        {
            lock (this)
            {
                action.Invoke(this.Data);
            }
        }
        public T Do<T>(Func<LiteDatabase, T> action)
        {
            lock (this)
            {
                return action.Invoke(this.Data);
            }
        }

        public IEnumerable<Anime> GetSeasonAnimes(LiteDatabase database, string season)
        {
            return database.GetCollection<Anime>("animes").Find(x => x.Season == season);
        }

        public Anime GetAnime(LiteDatabase database, long id)
        {
            return database.GetCollection<Anime>("animes").Find(x => x.Id == id).FirstOrDefault();
        }

        public void UpdateHosters(LiteDatabase database, Anime anime, HosterInformation[] hosters)
        {
            anime.HosterMinedAt = DateTime.UtcNow;
            anime.Hoster = hosters;
            database.GetCollection<Anime>("animes").Update(anime);
        }

        public void InsertSeasonAnimes(LiteDatabase database, Anime[] animes)
        {
            database.GetCollection<Anime>("animes").InsertBulk(animes);
        }

        public LiteCollection<Anime> GetAnimeCollection(LiteDatabase database)
        {
            return database.GetCollection<Anime>("animes");
        }

        public void UpdateSeasonAnimes(LiteDatabase database, Anime[] animes)
        {
            var animeDocuments = database.GetCollection<Anime>("animes");
            foreach (var anime in animes)
            {
                var matchingAnime = animeDocuments.FindOne(x => x.Mal.Id == anime.Mal.Id && x.Season == anime.Season);
                if (matchingAnime != null)
                {
                    // update only mal information
                    matchingAnime.Mal.EpisodesCount = anime.Mal.EpisodesCount;
                    matchingAnime.Mal.ImageUrl = anime.Mal.ImageUrl;
                    matchingAnime.Mal.MemberCount = anime.Mal.MemberCount;
                    matchingAnime.Mal.Name = anime.Mal.Name;
                    matchingAnime.Mal.Score = anime.Mal.Score;
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
        }
    }
}
