using System;
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

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddGrpcClient<SeasonBackend.Protos.SeasonProvider.SeasonProviderClient>("test", options =>
                {
                    var url = configuration.GetValue<string>("BackendUrl") ?? "";
                    options.Address = new Uri(url);
                });
            services.AddScoped<Data.AnimeSeasonService>();
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