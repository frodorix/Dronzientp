using CORE.Domain.Model;
using CORE.DTO;

namespace CORE.Interfaces
{
    public interface IPackageRepository
    {
        Task<string> createPlan(MTripPlan plan);
        Task<List<MTripPlanDTO>> GetLast10DeliveryPlan();
        Task<MTripPlan> getPlan(string? id);
        Task<MDrone> getDrone(string planid, int droneId);
    }
}