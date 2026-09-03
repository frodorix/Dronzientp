namespace DroneDeliveryApp.Infrastructure.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "DroneDeliveryDb";
    public string CollectionName { get; set; } = "DeliveryPlans";
}
