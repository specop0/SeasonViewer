using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SeasonViewer.Authentication;

// https://github.com/dotnet/blazor-samples/blob/main/8.0/BlazorWebAppOidcServer/BlazorWebAppOidcServer/LoginLogoutEndpointRouteBuilderExtensions.cs
public static class OidcWebApplicationExtensions
{
    public static void MapLoginAndLogout(this WebApplication app)
    {
        app.MapGet(
            "/login",
            (string? returnUrl, [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                return TypedResults.Challenge(GetAuthProperties(returnUrl, httpContextAccessor));
            })
            .AllowAnonymous();

        app.MapGet(
            "/logout",
            (string? returnUrl, [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                return TypedResults.SignOut(
                    GetAuthProperties(returnUrl, httpContextAccessor),
                    [CookieAuthenticationDefaults.AuthenticationScheme]
                );
            }
        );
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl, IHttpContextAccessor httpContextAccessor)
    {
        var pathBase = "/";
        if (httpContextAccessor.HttpContext is not null)
        {
            pathBase = httpContextAccessor.HttpContext.Request.PathBase;
        }

        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}