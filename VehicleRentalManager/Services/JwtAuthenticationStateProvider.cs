using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using VehicleRentalManager.Services;

namespace VehicleRentalManager.Services;

/// <summary>
/// FIX #8: Custom AuthenticationStateProvider that reads the JWT stored in the
/// "jwt" cookie (set by ExternalLoginCallback) and exposes it to Blazor
/// components via [CascadingParameter] AuthenticationState.
///
/// Without this, AuthenticationStateProvider always returns an anonymous user
/// because ASP.NET Identity's cookie auth state is not automatically propagated
/// into the Blazor SignalR circuit.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;

    public JwtAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Try to read the JWT from the cookie set by ExternalLoginCallback
        if (httpContext != null &&
            httpContext.Request.Cookies.TryGetValue("jwt", out var token) &&
            !string.IsNullOrEmpty(token))
        {
            var userId = _jwtService.ValidateToken(token);
            if (userId != null)
            {
                // Parse claims directly from the token for the Blazor identity
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            }
        }

        // No valid JWT — return anonymous
        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    /// <summary>
    /// Call this after storing a new token so Blazor components re-render.
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}