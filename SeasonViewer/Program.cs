using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeasonViewer.Authentication;

namespace SeasonViewer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            Configure(app, app.Configuration);
            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            services.AddHttpContextAccessor();

            services.AddAuthorization();
            services.AddCascadingAuthenticationState();
            services.AddOidcAuthentication(configuration);

            services.AddSingleton<SeasonViewer.Core.IDatabaseService, SeasonViewer.Infrastructure.Database.DatabaseService>();
            services.AddSingleton<SeasonViewer.Core.Hosters.IHosterService, SeasonViewer.Infrastructure.Hosters.HosterService>();
            services.AddHttpClient<SeasonViewer.Core.Miner.ISeleniumMiner, SeasonViewer.Infrastructure.Miner.SeleniumMiner>();
            services.AddScoped<SeasonViewer.Core.Services.SeasonService>();
            services.AddScoped<SeasonViewer.UserInterface.AnimeSeasonService>();
        }

        public static void Configure(WebApplication app, IConfiguration configuration)
        {
            var pathBase = configuration.GetValue<string>("PathBase");
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapApiEndpoints();
            app.MapLoginAndLogout();

            app.MapRazorComponents<Components.App>()
                .AddInteractiveServerRenderMode();
        }
    }
}