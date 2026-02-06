using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public record CurrentUserDto(string? Name, string? Email, bool IsAuthenticated);

    /// <summary>
    /// Stores the JWT token in local storage and updates the HttpClient Authorization header.
    /// Call this after a successful login (when the token comes from the URL).
    /// </summary>
    public async Task StoreTokenAsync(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Loads the token from local storage (if any) and applies it to HttpClient.
    /// This is useful when the app starts or the page reloads.
    /// </summary>
    public async Task InitializeFromStorageAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Gets the current authenticated user from the backend.
    /// </summary>
    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        try
        {
            // Ensure we are using the token stored in local storage
            await InitializeFromStorageAsync();

            var result = await _httpClient.GetFromJsonAsync<CurrentUserDto>("auth/me");
            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Logs out the current user:
    /// 1) Removes the token from local storage.
    /// 2) Clears the Authorization header.
    /// 3) Calls the backend logout endpoint (cookie-based).
    /// </summary>
    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        try
        {
            await _httpClient.PostAsync("auth/logout", null);
        }
        catch
        {
            // Ignore errors on logout call
        }
    }
}
