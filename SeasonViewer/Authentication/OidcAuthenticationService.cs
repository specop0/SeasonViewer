using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace SeasonViewer.Authentication;

public class OidcAuthenticationService
{
    public OidcAuthenticationService(
        IHttpContextAccessor httpContextAccessor,
        NavigationManager navigationManager)
    {
        this.HttpContext = httpContextAccessor.HttpContext;
        this.NavigationManager = navigationManager;
    }

    private HttpContext? HttpContext { get; }

    private NavigationManager NavigationManager { get; }

    public bool IsAuthenticated => this.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public void StartLogInNotAuthenticated()
    {
        if (this.IsAuthenticated)
        {
            return;
        }

        var currentUrl = this.NavigationManager.Uri;
        var url = "/login";
        if (currentUrl is not null)
        {
            var queryParameter = "returnUrl=" + Uri.EscapeDataString(currentUrl);
            url = $"{url}?{queryParameter}";
        }
        this.NavigationManager.NavigateTo(url);
    }
}