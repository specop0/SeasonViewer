using LiteDB;
using System;
using System.Collections.Generic;

namespace SeasonBackend.Database
{
    public class DatabaseAccess : IDisposable
    {
        public DatabaseAccess(string databaseName)
        {
            // https://www.litedb.org/
            this.Data = new LiteDatabase(databaseName);
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
