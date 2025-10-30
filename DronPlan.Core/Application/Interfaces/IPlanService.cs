using CORE.Domain.Model;
using CORE.DTO;

namespace CORE.Application.Interfaces
{
    /// <summary>
    /// Service interface for delivery plan management.
    /// Handles parsing input files, creating plans, and retrieving plan history.
    /// </summary>
    public interface IPlanService
    {
        /// <summary>
        /// Prepares a delivery plan for given drones and packages.
        /// </summary>
        /// <param name="drones">List of available drones</param>
        /// <param name="packages">List of packages to deliver</param>
        /// <returns>The created trip plan</returns>
        Task<MTripPlan> PrepareDeliveryPlan(List<MDrone> drones, List<MPackage> packages);
        
        /// <summary>
        /// Parses input data from a CSV file.
        /// </summary>
        /// <param name="csvFilename">Path to the CSV file</param>
        /// <returns>Tuple containing lists of drones and packages</returns>
        Tuple<List<MDrone>, List<MPackage>> ParseInputData(string csvFilename);
        
        /// <summary>
        /// Imports a CSV file and generates a delivery plan.
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>Formatted text representation of the plan</returns>
        Task<string> ImportFile(string filePath);
        
        /// <summary>
        /// Retrieves the last 10 delivery plans.
        /// </summary>
        /// <returns>List of trip plan summaries</returns>
        Task<List<MTripPlanDTO>> GetLast10DeliveryPlan();
        
        /// <summary>
        /// Retrieves a specific delivery plan by ID.
        /// </summary>
        /// <param name="id">The plan ID</param>
        /// <returns>The trip plan, or null if not found</returns>
        Task<MTripPlan?> getPlan(string? id);
        
        /// <summary>
        /// Retrieves a specific drone from a delivery plan.
        /// </summary>
        /// <param name="planId">The plan ID</param>
        /// <param name="droneId">The drone ID</param>
        /// <returns>The drone with its trips, or null if not found</returns>
        Task<MDrone?> getDrone(string planId, int droneId);
    }
}
