using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace VehicleRentalManager.Services;

public class AuthService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public record CurrentUserDto(string? Name, string? Email, bool IsAuthenticated);

    /// <summary>
    /// Returns the current user's info from Blazor auth state (backed by JWT cookie).
    /// </summary>
    public async Task<CurrentUserDto> GetCurrentUserAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var user  = state.User;

        if (user.Identity?.IsAuthenticated != true)
            return new CurrentUserDto(null, null, false);

        var name  = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                 ?? user.FindFirst(ClaimTypes.Name)?.Value;
        var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                 ?? user.FindFirst(ClaimTypes.Email)?.Value;

        return new CurrentUserDto(name, email, true);
    }

    /// <summary>
    /// Logs out by hitting the server logout endpoint which clears the cookie.
    /// Home.razor then calls Navigation.NavigateTo("/auth", forceLoad: true)
    /// to reload the circuit with no cookie.
    /// </summary>
    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }
}