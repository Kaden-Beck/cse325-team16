using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VehicleRentalManager.Models;
public class Vehicle
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("make")]
    public string Make { get; set; } = null!;

    [BsonElement("model")]
    public string Model { get; set; } = null!;

    [BsonElement("color")]
    public string Color { get; set; } = null!;

    [BsonElement("year")]
    public int Year { get; set; }
}
