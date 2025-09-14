using System;
using LiteDB;
using Microsoft.Extensions.Configuration;
using SeasonViewer.Core;
using SeasonViewer.Core.Hosters;

namespace SeasonViewer.Infrastructure.Database
{
    public class DatabaseService : IDatabaseService
    {
        public DatabaseService(IConfiguration configuration, IHosterService hosterService)
        {
            var databasePath = configuration.GetValue<string>("ConnectionStrings:FileDatabasePath");
            // https://www.litedb.org/
            this.Data = new LiteDatabase(databasePath);
            this.HosterService = hosterService;

            this.Do(databaseAccess =>
            {
                var databaseAccessInternal = (DatabaseContext)databaseAccess;
                var animeCollection = databaseAccessInternal.GetAnimeCollection();
                animeCollection.EnsureIndex(x => x.Id, unique: true);
                animeCollection.EnsureIndex(x => x.Seasons);
                animeCollection.EnsureIndex(x => x.Mal.Name);
                animeCollection.EnsureIndex(x => x.Mal.Id);

                var imageDataCollection = databaseAccessInternal.GetImageDataCollection();
                imageDataCollection.EnsureIndex(x => x.Id, unique: true);
            });

        }

        private static readonly object _lock = new();

        private LiteDatabase Data { get; }
        private IHosterService HosterService { get; }

        public void Do(Action<IDatabaseContext> action)
        {
            lock (_lock)
            {
                using var databaseAccess = new DatabaseContext(this.Data, this.HosterService);
                action.Invoke(databaseAccess);
            }
        }
        public T Do<T>(Func<IDatabaseContext, T> action)
        {
            lock (_lock)
            {
                using var databaseAccess = new DatabaseContext(this.Data, this.HosterService);
                return action.Invoke(databaseAccess);
            }
        }
        public void Dispose()
        {
            this.Data.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}