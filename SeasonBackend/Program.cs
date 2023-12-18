using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SeasonBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services);
            var app = builder.Build();
            Configure(app);
            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton<Database.DatabaseAccess>();
            services.AddSingleton<Services.HosterService>();
            services.AddSingleton<Miner.SeleniumMiner>();
        }

        public static void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapGrpcService<Services.SeasonService>();
            app.MapGet("/", () => "SeasonBackend is running");
        }
    }
}
