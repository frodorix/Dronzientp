using DroneDeliveryApp.Core.Models;

namespace DroneDeliveryApp.Core.Interfaces;

public interface IDeliveryPlanner
{
    DeliveryPlan CreateDeliveryPlan(List<Drone> drones, List<Package> packages, string fileName = "delivery_input.csv");
}

public interface ICsvParserService
{
    (List<Drone> Drones, List<Package> Packages) ParseCsv(Stream csvStream);
    (List<Drone> Drones, List<Package> Packages) ParseCsv(string csvContent);
}

public interface IDeliveryPlanRepository
{
    Task<DeliveryPlan> SaveAsync(DeliveryPlan plan, CancellationToken cancellationToken = default);
    Task<List<DeliveryPlan>> GetRecentPlansAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<DeliveryPlan?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
