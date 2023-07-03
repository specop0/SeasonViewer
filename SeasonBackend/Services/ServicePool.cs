using System;
using System.Collections.Generic;
using System.Linq;
using SeasonBackend.Database;
using SeasonBackend.Miner;

namespace SeasonBackend.Services
{
    public class ServicePool : IDisposable
    {
        private ServicePool()
        {
            this.Services = new Dictionary<Type, object>
            {
                { typeof(DatabaseAccess), new DatabaseAccess("MyData.db") },
                { typeof(SeleniumMiner), new SeleniumMiner() },
                { typeof(HosterService), new HosterService() },
            };
        }

        private static ServicePool instance;
        public static ServicePool Instance => instance ?? (instance = new ServicePool());

        private Dictionary<Type, object> Services { get; }

        public T GetService<T>()
        {
            this.Services.TryGetValue(typeof(T), out var service);
            return (T)service;
        }

        public void Dispose()
        {
            foreach (var disposable in this.Services.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}
