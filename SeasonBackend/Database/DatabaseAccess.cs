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

        public void Dispose()
        {
            this.Data?.Dispose();
        }
    }
}
