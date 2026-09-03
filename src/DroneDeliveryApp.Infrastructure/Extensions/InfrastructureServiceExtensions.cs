using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Infrastructure.Repositories;
using DroneDeliveryApp.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DroneDeliveryApp.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(options =>
        {
            var connStr = configuration.GetConnectionString("MongoDB")
                ?? configuration["MongoDbSettings:ConnectionString"]
                ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                ?? "mongodb://localhost:27017";

            var dbName = configuration["MongoDbSettings:DatabaseName"]
                ?? Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
                ?? "DroneDeliveryDb";

            var collName = configuration["MongoDbSettings:CollectionName"]
                ?? Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME")
                ?? "DeliveryPlans";

            options.ConnectionString = connStr;
            options.DatabaseName = dbName;
            options.CollectionName = collName;
        });

        services.AddSingleton<InMemoryDeliveryPlanRepository>();
        services.AddSingleton<IDeliveryPlanRepository, ResilientDeliveryPlanRepository>();

        return services;
    }
}
