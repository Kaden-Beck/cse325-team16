using MongoDB.Driver;
using MongoDB.Bson;

public class MongoContext
{
    public IMongoDatabase Database { get; }
    public MongoContext(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"];
        var dbName = config["MongoDB:Database"];

        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(dbName);
    }
}