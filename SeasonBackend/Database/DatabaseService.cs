using System;
using LiteDB;
using Microsoft.Extensions.Configuration;
using SeasonBackend.Services;

namespace SeasonBackend.Database
{
    public class DatabaseService
    {
        public DatabaseService(IConfiguration configuration, HosterService hosterService)
        {
            var databasePath = configuration.GetValue<string>("ConnectionStrings:FileDatabasePath");
            // https://www.litedb.org/
            this.Data = new LiteDatabase(databasePath);
            this.HosterService = hosterService;

            this.Do(databaseAccess =>
            {
                var animeCollection = databaseAccess.GetAnimeCollection();
                animeCollection.EnsureIndex(x => x.Seasons);
                animeCollection.EnsureIndex(x => x.Mal.Name);
                animeCollection.EnsureIndex(x => x.Mal.Id);
            });

        }

        private static readonly object _lock = new();

        private LiteDatabase Data { get; }
        private HosterService HosterService { get; }

        public void Do(Action<DatabaseContext> action)
        {
            lock (_lock)
            {
                using var databaseAccess = new DatabaseContext(this.Data, this.HosterService);
                action.Invoke(databaseAccess);
            }
        }
        public T Do<T>(Func<DatabaseContext, T> action)
        {
            lock (_lock)
            {
                using var databaseAccess = new DatabaseContext(this.Data, this.HosterService);
                return action.Invoke(new DatabaseContext(this.Data, this.HosterService));
            }
        }
        public void Dispose()
        {
            this.Data.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}