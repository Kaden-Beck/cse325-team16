using VehicleRentalManager.Models;

namespace VehicleRentalManager.Repositories
{
    /// <summary>
    /// Interface for user data access operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetByIdAsync(string id);

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="user">User to create</param>
        /// <returns>Created user</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">User to update</param>
        /// <returns>Updated user</returns>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Deletes a user (or marks as inactive)
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Gets all active users
        /// </summary>
        /// <returns>List of active users</returns>
        Task<IEnumerable<User>> GetAllActiveAsync();
    }
}