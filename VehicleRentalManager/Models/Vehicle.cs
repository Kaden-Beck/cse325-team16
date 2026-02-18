using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VehicleRentalManager.Models;

public class Vehicle
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("make")]
    [Required(ErrorMessage = "Make is required.")]
    [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters.")]
    public string Make { get; set; } = string.Empty;

    [BsonElement("model")]
    [Required(ErrorMessage = "Model is required.")]
    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters.")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("color")]
    [Required(ErrorMessage = "Color is required.")]
    [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters.")]
    public string Color { get; set; } = string.Empty;

    [BsonElement("year")]
    [Required(ErrorMessage = "Year is required.")]
    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int? Year { get; set; }

    [BsonElement("pricePerDay")]
    [Required(ErrorMessage = "Price per day is required.")]
    [Range(0.01, 10000, ErrorMessage = "Price per day must be greater than 0.")]
    public decimal? PricePerDay { get; set; }

    [BsonElement("isAvailable")]
    public bool IsAvailable { get; set; }

    [BsonElement("licensePlate")]
    [Required(ErrorMessage = "License plate is required.")]
    [RegularExpression(@"^[A-Z]{3}-[0-9]{3}$",
    ErrorMessage = "License plate must follow the format: ABC-123.")]
    public string LicensePlate { get; set; } = string.Empty;

}
