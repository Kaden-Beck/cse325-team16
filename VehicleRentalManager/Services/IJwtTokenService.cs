using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services
{
    /// <summary>
    /// Interface for JWT token operations
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        string GenerateToken(User user);

        /// <summary>
        /// Validates a JWT token and returns the user ID if valid
        /// </summary>
        string? ValidateToken(string token);
    }
}