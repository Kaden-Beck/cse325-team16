namespace VehicleRentalManager.Services;
using MongoDB.Driver;
using VehicleRentalManager.Models;

public class VehicleService
{
    private readonly IMongoCollection<Vehicle> _vehicles;

    public VehicleService(MongoContext context)
    {
        _vehicles = context.Database.GetCollection<Vehicle>("Vehicle");
    }

    public Task<List<Vehicle>> GetAllAsync()
        => _vehicles.Find(_ => true).ToListAsync();

    // public Task CreateAsync(Vehicle v)
    //     => _vehicles.InsertOneAsync(v);
}
