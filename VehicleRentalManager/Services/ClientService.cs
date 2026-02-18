using MongoDB.Driver;
using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public class ClientService
{
    private readonly IMongoCollection<Client> _clients;

    public ClientService(MongoDbService mongoDbService)
    {
        _clients = mongoDbService.GetCollection<Client>("clients");
    }

    public async Task<List<Client>> GetAsync()
    {
        return await _clients.Find(_ => true).SortBy(c => c.Name).ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(string id)
    {
        return await _clients.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Client?> GetByEmailAsync(string email)
    {
        return await _clients.Find(c => c.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Client client)
    {
        client.CreatedAt = DateTime.UtcNow;
        await _clients.InsertOneAsync(client);
    }

    public async Task UpdateAsync(string id, Client client)
    {
        await _clients.ReplaceOneAsync(c => c.Id == id, client);
    }

    public async Task DeleteAsync(string id)
    {
        await _clients.DeleteOneAsync(c => c.Id == id);
    }

    public async Task UpdateLastRentalDateAsync(string id)
    {
        var update = Builders<Client>.Update.Set(c => c.LastRentalDate, DateTime.UtcNow);
        await _clients.UpdateOneAsync(c => c.Id == id, update);
    }
}