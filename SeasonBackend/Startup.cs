using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeasonBackend.Services;

namespace SeasonBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var environmentVariables = configuration.GetSection("EnvironmentVariables").GetChildren();
            foreach (var environmentVariable in environmentVariables)
            {
                var key = environmentVariable.Key;
                var currentValue = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrEmpty(currentValue))
                {
                    Environment.SetEnvironmentVariable(key, environmentVariable.Value);
                }
            }

            ServicePool.Instance.GetService<HosterService>().Initialize(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<SeasonService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
