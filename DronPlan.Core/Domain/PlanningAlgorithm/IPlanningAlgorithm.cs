using CORE.Domain.Model;

namespace CORE.Domain.PlanningAlgorithm
{
    /// <summary>
    /// Interface for delivery planning algorithms.
    /// Implementations should assign packages to drone trips optimally.
    /// </summary>
    public interface IPlanningAlgorithm
    {
        /// <summary>
        /// Prepares a delivery plan by assigning packages to drones.
        /// </summary>
        /// <param name="drones">List of available drones</param>
        /// <param name="packages">List of packages to be delivered</param>
        /// <returns>A complete trip plan with package assignments</returns>
        MTripPlan PrepareDeliveryPlan(List<MDrone> drones, List<MPackage> packages);
    }
}
