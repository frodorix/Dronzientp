using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Models;
using DroneDeliveryApp.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DroneDeliveryApp.Infrastructure.Repositories;

public class ResilientDeliveryPlanRepository : IDeliveryPlanRepository
{
    private readonly MongoDeliveryPlanRepository? _mongoRepo;
    private readonly InMemoryDeliveryPlanRepository _inMemoryRepo;
    private readonly ILogger<ResilientDeliveryPlanRepository> _logger;
    private bool _useMongo;

    public ResilientDeliveryPlanRepository(
        IOptions<MongoDbSettings> settings,
        InMemoryDeliveryPlanRepository inMemoryRepo,
        ILogger<ResilientDeliveryPlanRepository> logger)
    {
        _inMemoryRepo = inMemoryRepo;
        _logger = logger;

        try
        {
            if (!string.IsNullOrWhiteSpace(settings.Value.ConnectionString))
            {
                _mongoRepo = new MongoDeliveryPlanRepository(settings);
                _useMongo = true;
                _logger.LogInformation("Initialized MongoDB repository with connection string.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize MongoDB repository. Falling back to In-Memory repository.");
            _useMongo = false;
        }
    }

    public async Task<DeliveryPlan> SaveAsync(DeliveryPlan plan, CancellationToken cancellationToken = default)
    {
        if (_useMongo && _mongoRepo != null)
        {
            try
            {
                var saved = await _mongoRepo.SaveAsync(plan, cancellationToken);
                // Also cache in memory for fast lookup
                await _inMemoryRepo.SaveAsync(plan, cancellationToken);
                return saved;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB SaveAsync failed. Falling back to In-Memory repository.");
                _useMongo = false;
            }
        }

        return await _inMemoryRepo.SaveAsync(plan, cancellationToken);
    }

    public async Task<List<DeliveryPlan>> GetRecentPlansAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        if (_useMongo && _mongoRepo != null)
        {
            try
            {
                return await _mongoRepo.GetRecentPlansAsync(count, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB GetRecentPlansAsync failed. Falling back to In-Memory repository.");
                _useMongo = false;
            }
        }

        return await _inMemoryRepo.GetRecentPlansAsync(count, cancellationToken);
    }

    public async Task<DeliveryPlan?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_useMongo && _mongoRepo != null)
        {
            try
            {
                var result = await _mongoRepo.GetByIdAsync(id, cancellationToken);
                if (result != null) return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB GetByIdAsync failed. Falling back to In-Memory repository.");
                _useMongo = false;
            }
        }

        return await _inMemoryRepo.GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_useMongo && _mongoRepo != null)
        {
            try
            {
                await _mongoRepo.DeleteAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB DeleteAsync failed. Falling back to In-Memory repository.");
                _useMongo = false;
            }
        }

        return await _inMemoryRepo.DeleteAsync(id, cancellationToken);
    }
}
