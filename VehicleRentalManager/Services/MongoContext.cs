using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

public class MongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(IConfiguration config)
    {
        var connectionString = config["MongoDb:ConnectionString"];
        var dbName = config["MongoDb:DatabaseName"];

        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(dbName);
    }
}
