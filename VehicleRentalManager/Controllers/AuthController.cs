using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;

namespace VehicleRentalManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJwtTokenService _jwtTokenService;
        // TODO: Inject your user repository/service here when database is ready
        // private readonly IUserRepository _userRepository;

        public AuthController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IJwtTokenService jwtTokenService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Gets public OAuth configuration needed for client-side authentication
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetOAuthConfig()
        {
            // Try to get from IConfiguration first, then fallback to environment variables (from .env)
            var clientId = _configuration["GoogleOAuth:ClientId"] 
                ?? Environment.GetEnvironmentVariable("GoogleOAuth__ClientId")
                ?? Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
            
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"] 
                ?? Environment.GetEnvironmentVariable("GoogleOAuth__RedirectUri");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest("OAuth configuration is not properly set");
            }

            return Ok(new OAuthConfigResponse
            {
                ClientId = clientId,
                RedirectUri = redirectUri
            });
        }

        /// <summary>
        /// Handles OAuth callback from Google
        /// Processes both registration and login based on the action in state parameter
        /// </summary>
        [HttpPost("oauth-callback")]
        public async Task<IActionResult> OAuthCallback([FromBody] OAuthCallbackRequest request)
        {
            try
            {
                // 1. Exchange authorization code for access token
                var tokenResponse = await ExchangeCodeForTokenAsync(request.Code);

                if (tokenResponse == null)
                {
                    return BadRequest("Failed to exchange authorization code for token.");
                }

                // 2. Get user information from Google
                var googleUser = await GetGoogleUserInfoAsync(tokenResponse.AccessToken);

                if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
                {
                    return BadRequest("Failed to retrieve user information from Google.");
                }

                // 3. Process based on action (register or login)
                if (request.Action == "register")
                {
                    return await HandleRegistrationAsync(googleUser);
                }
                else if (request.Action == "login")
                {
                    return await HandleLoginAsync(googleUser);
                }
                else
                {
                    return BadRequest("Invalid action specified.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OAuth callback error: {ex.Message}");
                return StatusCode(500, "Authentication failed. Please try again.");
            }
        }

        /// <summary>
        /// Handles user registration
        /// </summary>
        private async Task<IActionResult> HandleRegistrationAsync(GoogleUserInfo googleUser)
        {
            // Check if user already exists
            // TODO: Uncomment when database is ready
            // var existingUser = await _userRepository.GetByEmailAsync(googleUser.Email);

            // For now, proceed with registration
            var existingUser = (User?)null;

            if (existingUser != null)
            {
                return BadRequest("Account already exists. Please login.");
            }

            // Create new user
            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = googleUser.Name,
                Email = googleUser.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save user to database
            // TODO: Uncomment when database is ready
            // await _userRepository.CreateAsync(newUser);

            // For now, just log the action
            Console.WriteLine($"Save new user to database - Email: {newUser.Email}");

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(newUser);

            return Ok(new AuthResponse
            {
                Token = token,
                User = newUser
            });
        }

        /// <summary>
        /// Handles user login
        /// </summary>
        private async Task<IActionResult> HandleLoginAsync(GoogleUserInfo googleUser)
        {
            // Get user from database
            // TODO: Uncomment when database is ready
            // var user = await _userRepository.GetByEmailAsync(googleUser.Email);

            // For now, create a temporary user object
            var user = new User
            {
                Id = googleUser.Id,
                Name = googleUser.Name,
                Email = googleUser.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (user == null)
            {
                return BadRequest("Account not found. Please register first.");
            }

            if (!user.IsActive)
            {
                return BadRequest("Account is inactive. Please contact support.");
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = user
            });
        }

        /// <summary>
        /// Exchanges Google authorization code for access token
        /// </summary>
        private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var tokenEndpoint = "https://oauth2.googleapis.com/token";

                var googleClientId = _configuration["GoogleOAuth:ClientId"];
                var googleClientSecret = _configuration["GoogleOAuth:ClientSecret"];
                var redirectUri = _configuration["GoogleOAuth:RedirectUri"];

                var parameters = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", googleClientId },
                    { "client_secret", googleClientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync(tokenEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Token exchange failed: {errorContent}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return tokenResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exchanging code for token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets user information from Google using access token
        /// </summary>
        private async Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get user info: {errorContent}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Get user ID from JWT token claims
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");

                if (userIdClaim == null)
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = userIdClaim.Value;

                // TODO: Uncomment when database is ready
                // var user = await _userRepository.GetByIdAsync(userId);
                
                // For now, return a dummy user
                var user = new User { Id = userId, Email = "user@example.com", Name = "User" };

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current user: {ex.Message}");
                return StatusCode(500, "Failed to retrieve user information.");
            }
        }

        /// <summary>
        /// Logout endpoint (optional - mainly for token invalidation if implementing refresh tokens)
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }
    }

    #region Request/Response Models

    public class OAuthCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "register" or "login"
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }

    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }

    public class GoogleUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }
    }

    public class OAuthConfigResponse
    {
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }

    #endregion
}