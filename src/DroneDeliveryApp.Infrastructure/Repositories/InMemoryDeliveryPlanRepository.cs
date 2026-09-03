using System.Collections.Concurrent;
using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Models;

namespace DroneDeliveryApp.Infrastructure.Repositories;

public class InMemoryDeliveryPlanRepository : IDeliveryPlanRepository
{
    private static readonly ConcurrentDictionary<string, DeliveryPlan> _plans = new();

    public Task<DeliveryPlan> SaveAsync(DeliveryPlan plan, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plan.Id))
        {
            plan.Id = Guid.NewGuid().ToString("N");
        }

        _plans[plan.Id] = plan;
        return Task.FromResult(plan);
    }

    public Task<List<DeliveryPlan>> GetRecentPlansAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        var recent = _plans.Values
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToList();

        return Task.FromResult(recent);
    }

    public Task<DeliveryPlan?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _plans.TryGetValue(id, out var plan);
        return Task.FromResult(plan);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        bool removed = _plans.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}
