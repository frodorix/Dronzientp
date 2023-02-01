using CORE.Domain.Model;
using CORE.DTO;

namespace CORE.Application.Interfaces
{
    public interface IPlanService
    {
        Task<MTripPlan> PrepareDeliveryPlan(List<MDrone> dron, List<MPackage> pack);
        Tuple<List<MDrone>, List<MPackage>> ParseInputData(string csvFilename);
        Task<string> ImportFile(string filePath);
        Task<List<MTripPlanDTO>> GetLast10DeliveryPlan();
        Task<MTripPlan> getPlan(string? id);
        Task<MDrone> getDrone(string planid, int droneId);
    }
}
