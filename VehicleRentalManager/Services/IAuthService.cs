using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services
{
    /// <summary>
    /// Interface for authentication service operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with Google OAuth
        /// Creates a new account and logs in
        /// </summary>
        Task RegisterWithGoogleAsync();

        /// <summary>
        /// Logs in an existing user with Google OAuth
        /// Throws exception if account doesn't exist
        /// </summary>
        Task LoginWithGoogleAsync();

        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        Task<User?> GetCurrentUserAsync();
    }
}