namespace VehicleRentalManager.Models
{
    /// <summary>
    /// Model representing a user in the system
    /// </summary>
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}