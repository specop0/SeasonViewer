using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SeasonBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Services.ServicePool.Instance)
            {
                CreateHostBuilder(args).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
