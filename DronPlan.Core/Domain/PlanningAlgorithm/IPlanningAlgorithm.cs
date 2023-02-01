using CORE.Domain.Model;

namespace CORE.Domain.PlanningAlgorithm
{
    public interface IPlanningAlgorithm
    {
        MTripPlan PrepareDeliveryPlan(List<MDrone> drone, List<MPackage> sortedPackages);
    }
}