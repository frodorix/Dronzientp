using CORE.Domain.Model;
using CORE.Domain.PlanningAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Application.PlanningAlgorithm
{
    public class CustomGreedyAlgorithm : IPlanningAlgorithm
    {
        /// <summary>
        ///         
        /// </summary>
        /// <returns>List of Drones wind assigned trips</returns>
        public MTripPlan PrepareDeliveryPlan(List<MDrone> drone, List<MPackage> packages)
        {

            //Sorting the drones to use first the bigger ones
            var droneSorted = drone.OrderByDescending(d => d.MaxWeight).ToList();
            //grouping packages in order to assing duplicated locations to the same drone and reduce de trips to the same location
            var packageGroups = packages.GroupBy(x => x.Location)
                .Select(x => new { Location = x.Key, Count = x.Count(), TotalWeight = x.Sum(y => y.Weight) });
            ///packages are sorted ascending by duplicated locations, then  by  Grouped weight in ascending order, 
            ///then by package weight ascending, in order to assing first the lightest packages             
            var sortedPackages = (from p in packages
                                  join g in packageGroups on p.Location equals g.Location
                                  orderby g.Count ascending, g.TotalWeight ascending, p.Weight ascending
                                  select p).ToList();
            // it provides a better performance with ascending order when there are duplicated locations


            bool usedDrons = true;

            while (sortedPackages.Count() > 0 && usedDrons)

            {
                usedDrons = false;

                foreach (var dron in droneSorted)
                {
                    int totalWeight = 0;
                    var trip = new List<MPackage>();
                    for (int i = 0; i < sortedPackages.Count && dron.MaxWeight > totalWeight; i++)
                    {
                        var unassingedPackage = sortedPackages[i];
                        if (unassingedPackage.Weight <= dron.MaxWeight - totalWeight)
                        {
                            sortedPackages.RemoveAt(i);
                            i--;

                            trip.Add(unassingedPackage);
                            totalWeight += unassingedPackage.Weight;
                        }
                    }
                    if (trip.Count > 0)
                    {
                        usedDrons = true;
                        dron.Trips.Add(trip);
                    }
                }
            }
            var plan = new MTripPlan(droneSorted);
            return plan;
        }

    }
}
