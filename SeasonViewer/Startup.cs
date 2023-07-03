using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeasonViewer.Data;

namespace SeasonViewer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var environmentVariables = this.Configuration.GetSection("EnvironmentVariables").GetChildren();
            foreach (var environmentVariable in environmentVariables)
            {
                var key = environmentVariable.Key;
                var currentValue = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrEmpty(currentValue))
                {
                    Environment.SetEnvironmentVariable(key, environmentVariable.Value);
                }
            }
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<AnimeSeasonService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var pathBase = Environment.GetEnvironmentVariable("pathBase");
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(new PathString(pathBase));
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
