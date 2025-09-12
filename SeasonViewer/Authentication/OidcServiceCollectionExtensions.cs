using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace SeasonViewer.Authentication;

// https://github.com/dotnet/blazor-samples/blob/main/8.0/BlazorWebAppOidcServer/BlazorWebAppOidcServer/CookieOidcServiceCollectionExtensions.cs
internal static partial class OidcServiceCollectionExtensions
{
    public static IServiceCollection AddOidcAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddOpenIdConnect(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = configuration.GetValue<string>("Authentication:Authority") ?? "";
                    options.ClientId = configuration.GetValue<string>("Authentication:ClientId") ?? "";

                    options.SkipUnrecognizedRequests = true;
                }
            )
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        services.ConfigureOidcCookie(
            CookieAuthenticationDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme
        );

        services.AddScoped<OidcAuthenticationService>();

        return services;
    }

    public static IServiceCollection ConfigureOidcCookie(this IServiceCollection services, string cookieScheme, string oidcScheme)
    {
        services.AddSingleton<OidcCookieRefresher>();
        services.AddOptions<CookieAuthenticationOptions>(cookieScheme).Configure<OidcCookieRefresher>((options, refresher) =>
        {
            options.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, oidcScheme);
        });
        services.AddOptions<OpenIdConnectOptions>(oidcScheme).Configure(options =>
        {
            options.Scope.Add(OpenIdConnectScope.OfflineAccess);
            options.SaveTokens = true;
        });
        return services;
    }
}