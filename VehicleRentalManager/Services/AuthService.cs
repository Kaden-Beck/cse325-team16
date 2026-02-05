using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using System.Net.Http.Json;
using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services
{
    /// <summary>
    /// Implementation of authentication service with Google OAuth only
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILocalStorageService _localStorage;
        private User? _currentUser;

        public AuthService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorage = localStorage;
        }

        /// <summary>
        /// Registers a new user with Google OAuth
        /// </summary>
        public async Task RegisterWithGoogleAsync()
        {
            try
            {
                // Get OAuth configuration from server
                var config = await _httpClient.GetFromJsonAsync<OAuthConfigResponse>("/api/auth/config");
                if (config == null)
                {
                    throw new InvalidOperationException("Failed to get OAuth configuration");
                }

                // Build OAuth URL for registration
                string state = GenerateState("register");

                // Store state in localStorage for validation
                await _localStorage.SetItemAsync("oauth_state", state);

                string authorizationUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                    $"client_id={config.ClientId}&" +
                    $"redirect_uri={Uri.EscapeDataString(config.RedirectUri)}&" +
                    $"response_type=code&" +
                    $"scope=openid%20email%20profile&" +
                    $"state={state}&" +
                    $"access_type=offline&" +
                    $"prompt=consent";

                // Redirect to Google OAuth
                _navigationManager.NavigateTo(authorizationUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                throw new Exception("Failed to initiate registration. Please try again.");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Logs in an existing user with Google OAuth
        /// </summary>
        public async Task LoginWithGoogleAsync()
        {
            try
            {
                // Get OAuth configuration from server
                var config = await _httpClient.GetFromJsonAsync<OAuthConfigResponse>("/api/auth/config");
                if (config == null)
                {
                    throw new InvalidOperationException("Failed to get OAuth configuration");
                }

                // Build OAuth URL for login
                string state = GenerateState("login");

                // Store state in localStorage for validation
                await _localStorage.SetItemAsync("oauth_state", state);

                string authorizationUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                    $"client_id={config.ClientId}&" +
                    $"redirect_uri={Uri.EscapeDataString(config.RedirectUri)}&" +
                    $"response_type=code&" +
                    $"scope=openid%20email%20profile&" +
                    $"state={state}";

                // Redirect to Google OAuth
                _navigationManager.NavigateTo(authorizationUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw new Exception("Failed to initiate login. Please try again.");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                // Call backend logout endpoint
                await _httpClient.PostAsync("/api/auth/logout", null);

                // Clear current user
                _currentUser = null;

                // Clear stored token
                await _localStorage.RemoveItemAsync("authToken");

                // Navigate to auth page
                _navigationManager.NavigateTo("/auth");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");

                // Even if API call fails, clear local data
                _currentUser = null;
                await _localStorage.RemoveItemAsync("authToken");
                _navigationManager.NavigateTo("/auth");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        public async Task<User?> GetCurrentUserAsync()
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            try
            {
                // Get token from localStorage
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                // Set authorization header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get user from API
                var response = await _httpClient.GetAsync("/api/auth/me");

                if (response.IsSuccessStatusCode)
                {
                    _currentUser = await response.Content.ReadFromJsonAsync<User>();
                    return _currentUser;
                }
                else
                {
                    // Token might be expired or invalid
                    await _localStorage.RemoveItemAsync("authToken");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates a secure state parameter for OAuth
        /// State contains the action (register/login) and a random token for CSRF protection
        /// </summary>
        private string GenerateState(string action)
        {
            var randomBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var randomToken = Convert.ToBase64String(randomBytes);

            // Format: action|randomToken
            return $"{action}|{randomToken}";
        }
    }

    /// <summary>
    /// Response model for OAuth configuration
    /// </summary>
    public class OAuthConfigResponse
    {
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}