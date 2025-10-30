using CORE.Domain.Model;
using CORE.Domain.PlanningAlgorithm;

namespace CORE.Application.PlanningAlgorithm
{
    /// <summary>
    /// Custom greedy algorithm implementation for drone delivery planning.
    /// Optimizes package assignment by prioritizing location grouping and drone capacity utilization.
    /// </summary>
    public class CustomGreedyAlgorithm : IPlanningAlgorithm
    {
        /// <summary>
        /// Prepares a delivery plan by assigning packages to drones using a custom greedy approach.
        /// The algorithm sorts drones by capacity (descending) and packages by location frequency and weight (ascending)
        /// to minimize trips to the same location and maximize drone utilization.
        /// </summary>
        /// <param name="drones">List of available drones with their capacities</param>
        /// <param name="packages">List of packages to be delivered</param>
        /// <returns>A trip plan with packages assigned to drone trips</returns>
        public MTripPlan PrepareDeliveryPlan(List<MDrone> drones, List<MPackage> packages)
        {
            // Sort drones by capacity (descending) to use larger drones first
            var sortedDrones = drones.OrderByDescending(d => d.MaxWeight).ToList();
            
            // Group packages by location to identify delivery hotspots
            var packageGroups = packages.GroupBy(x => x.Location)
                .Select(x => new { 
                    Location = x.Key, 
                    Count = x.Count(), 
                    TotalWeight = x.Sum(y => y.Weight) 
                })
                .ToList();
            
            // Sort packages by:
            // 1. Location frequency (ascending) - less frequent locations first
            // 2. Grouped weight (ascending) - lighter groups first
            // 3. Individual weight (ascending) - lighter packages first
            // This optimizes for better packing and reduces trips to same location
            var sortedPackages = (from p in packages
                                  join g in packageGroups on p.Location equals g.Location
                                  orderby g.Count ascending, g.TotalWeight ascending, p.Weight ascending
                                  select p).ToList();

            // Convert to HashSet for O(1) removal performance instead of List.RemoveAt(i)
            var unassignedPackages = new HashSet<MPackage>(sortedPackages);
            
            // Continue while there are packages and drones can still be used
            bool dronesUsed = true;
            while (unassignedPackages.Count > 0 && dronesUsed)
            {
                dronesUsed = false;

                foreach (var drone in sortedDrones)
                {
                    double tripWeight = 0;
                    var trip = new List<MPackage>();
                    
                    // Use a temporary list to track packages to remove (avoid modifying HashSet during iteration)
                    var packagesToAssign = new List<MPackage>();
                    
                    foreach (var package in sortedPackages.Where(p => unassignedPackages.Contains(p)))
                    {
                        double remainingCapacity = drone.MaxWeight - tripWeight;
                        
                        if (package.Weight <= remainingCapacity)
                        {
                            packagesToAssign.Add(package);
                            tripWeight += package.Weight;
                        }
                        
                        // Stop if drone is at capacity
                        if (tripWeight >= drone.MaxWeight)
                        {
                            break;
                        }
                    }
                    
                    // Remove assigned packages from unassigned set
                    foreach (var package in packagesToAssign)
                    {
                        unassignedPackages.Remove(package);
                        trip.Add(package);
                    }
                    
                    // Add trip to drone if it has packages
                    if (trip.Count > 0)
                    {
                        dronesUsed = true;
                        drone.Trips.Add(trip);
                    }
                }
            }
            
            return new MTripPlan(sortedDrones);
        }
    }
}
