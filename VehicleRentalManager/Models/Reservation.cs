using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManager.Models;

public class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Client is required")]
    public string ClientId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vehicle is required")]
    public string VehicleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    public decimal TotalPrice { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    // Navigation properties (populated at runtime, not stored in DB)
    [BsonIgnore]
    public Client? Client { get; set; }

    [BsonIgnore]
    public Vehicle? Vehicle { get; set; }
}

public enum ReservationStatus
{
    Active,
    Completed,
    Cancelled
}