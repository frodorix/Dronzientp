using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Models;
using DroneDeliveryApp.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace DroneDeliveryApp.Infrastructure.Repositories;

public class MongoDeliveryPlanRepository : IDeliveryPlanRepository
{
    private readonly IMongoCollection<DeliveryPlan> _collection;

    static MongoDeliveryPlanRepository()
    {
        // Register BsonClassMap for DeliveryPlan if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(DeliveryPlan)))
        {
            BsonClassMap.RegisterClassMap<DeliveryPlan>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                  .SetSerializer(new StringSerializer(BsonType.String))
                  .SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }
    }

    public MongoDeliveryPlanRepository(IOptions<MongoDbSettings> settings)
    {
        var mongoSettings = settings.Value;
        var client = new MongoClient(mongoSettings.ConnectionString);
        var database = client.GetDatabase(mongoSettings.DatabaseName);
        _collection = database.GetCollection<DeliveryPlan>(mongoSettings.CollectionName);
    }

    public async Task<DeliveryPlan> SaveAsync(DeliveryPlan plan, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plan.Id))
        {
            plan.Id = Guid.NewGuid().ToString("N");
        }

        await _collection.ReplaceOneAsync(
            filter: p => p.Id == plan.Id,
            replacement: plan,
            options: new ReplaceOptions { IsUpsert = true },
            cancellationToken: cancellationToken
        );

        return plan;
    }

    public async Task<List<DeliveryPlan>> GetRecentPlansAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(Builders<DeliveryPlan>.Filter.Empty)
            .SortByDescending(p => p.CreatedAt)
            .Limit(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeliveryPlan?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteOneAsync(p => p.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
