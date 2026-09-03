using System.Diagnostics;
using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Models;

namespace DroneDeliveryApp.Core.Services;

public class DeliveryPlanner : IDeliveryPlanner
{
    public DeliveryPlan CreateDeliveryPlan(List<Drone> drones, List<Package> packages, string fileName = "delivery_input.csv")
    {
        var stopwatch = Stopwatch.StartNew();

        var plan = new DeliveryPlan
        {
            FileName = fileName,
            TotalDrones = drones.Count,
            TotalPackages = packages.Count,
            CreatedAt = DateTime.UtcNow
        };

        if (drones.Count == 0 || packages.Count == 0)
        {
            stopwatch.Stop();
            plan.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            return plan;
        }

        // 1. Sort drones by capacity descending (biggest capacity first)
        var sortedDrones = drones
            .Where(d => d.Capacity > 0)
            .OrderByDescending(d => d.Capacity)
            .ThenBy(d => d.Name)
            .ToList();

        if (sortedDrones.Count == 0)
        {
            foreach (var pkg in packages)
            {
                plan.UnassignedPackages.Add(new UnassignedPackage(pkg, "No drones with valid capacity available."));
            }
            stopwatch.Stop();
            plan.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            return plan;
        }

        double maxDroneCapacity = sortedDrones[0].Capacity;
        var unassignedPackages = new List<UnassignedPackage>();
        var deliverablePackages = new List<Package>();

        // 2. Separate overweight packages that no drone can carry
        foreach (var pkg in packages)
        {
            if (pkg.Weight <= 0)
            {
                unassignedPackages.Add(new UnassignedPackage(pkg, "Package weight must be greater than 0."));
            }
            else if (pkg.Weight > maxDroneCapacity)
            {
                unassignedPackages.Add(new UnassignedPackage(pkg, $"Package weight ({pkg.Weight}) exceeds maximum available drone capacity ({maxDroneCapacity})."));
            }
            else
            {
                deliverablePackages.Add(pkg);
            }
        }

        plan.UnassignedPackages.AddRange(unassignedPackages);

        // Group deliverable packages by location
        // Key = Location, Value = List of packages bound for that location
        var pendingByLocation = deliverablePackages
            .GroupBy(p => p.Location, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.Weight).ToList(),
                StringComparer.OrdinalIgnoreCase
            );

        // Initialize Drone Plans for all available drones
        var dronePlans = sortedDrones.ToDictionary(
            d => d.Name,
            d => new DronePlan { DroneName = d.Name, Capacity = d.Capacity },
            StringComparer.OrdinalIgnoreCase
        );

        var droneTripCounters = sortedDrones.ToDictionary(d => d.Name, d => 1, StringComparer.OrdinalIgnoreCase);

        // 3. Plan trips iteratively until all deliverable packages are assigned
        bool assignedAnyInRound = true;
        while (pendingByLocation.Any(kvp => kvp.Value.Count > 0) && assignedAnyInRound)
        {
            assignedAnyInRound = false;

            // Iterate over drones in order of largest capacity first
            foreach (var drone in sortedDrones)
            {
                // Check if there are any packages that fit in this drone
                if (!HasAnyFittingPackage(pendingByLocation, drone.Capacity))
                {
                    continue;
                }

                double remainingCapacity = drone.Capacity;
                var tripPackages = new List<Package>();
                string primaryLocation = string.Empty;

                // Step A: Pick the best primary location for this trip
                // Prefer location with the highest total weight of packages that fit in remainingCapacity
                var bestLocationKey = SelectBestPrimaryLocation(pendingByLocation, remainingCapacity);

                if (!string.IsNullOrEmpty(bestLocationKey))
                {
                    primaryLocation = bestLocationKey;
                    var locPackages = pendingByLocation[bestLocationKey];

                    // Greedily take packages from primary location that fit
                    for (int i = locPackages.Count - 1; i >= 0; i--)
                    {
                        var pkg = locPackages[i];
                        if (pkg.Weight <= remainingCapacity)
                        {
                            tripPackages.Add(pkg);
                            remainingCapacity -= pkg.Weight;
                            locPackages.RemoveAt(i);
                        }
                    }

                    if (locPackages.Count == 0)
                    {
                        pendingByLocation.Remove(bestLocationKey);
                    }
                }

                // Step B: If space remains in drone, try to pack packages for other locations to maximize utilization
                if (remainingCapacity > 0)
                {
                    var otherLocationKeys = pendingByLocation.Keys.ToList();
                    foreach (var locKey in otherLocationKeys)
                    {
                        if (remainingCapacity <= 0) break;
                        if (!pendingByLocation.TryGetValue(locKey, out var locPkgs)) continue;

                        for (int i = locPkgs.Count - 1; i >= 0; i--)
                        {
                            var pkg = locPkgs[i];
                            if (pkg.Weight <= remainingCapacity)
                            {
                                tripPackages.Add(pkg);
                                remainingCapacity -= pkg.Weight;
                                locPkgs.RemoveAt(i);

                                if (string.IsNullOrEmpty(primaryLocation))
                                {
                                    primaryLocation = locKey;
                                }
                            }
                        }

                        if (locPkgs.Count == 0)
                        {
                            pendingByLocation.Remove(locKey);
                        }
                    }
                }

                if (tripPackages.Count > 0)
                {
                    assignedAnyInRound = true;
                    int tripNum = droneTripCounters[drone.Name]++;

                    var trip = new DeliveryTrip
                    {
                        TripNumber = tripNum,
                        DroneName = drone.Name,
                        PrimaryLocation = primaryLocation,
                        Capacity = drone.Capacity,
                        Packages = tripPackages
                    };

                    dronePlans[drone.Name].Trips.Add(trip);
                }
            }
        }

        // Remaining unassigned packages (if any)
        foreach (var kvp in pendingByLocation)
        {
            foreach (var pkg in kvp.Value)
            {
                plan.UnassignedPackages.Add(new UnassignedPackage(pkg, "Could not be assigned within available drone trips."));
            }
        }

        plan.DronePlans = dronePlans.Values.Where(dp => dp.Trips.Count > 0).ToList();

        stopwatch.Stop();
        plan.ExecutionTimeMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
        return plan;
    }

    private static bool HasAnyFittingPackage(Dictionary<string, List<Package>> pendingByLocation, double droneCapacity)
    {
        foreach (var list in pendingByLocation.Values)
        {
            if (list.Any(p => p.Weight <= droneCapacity))
                return true;
        }
        return false;
    }

    private static string? SelectBestPrimaryLocation(Dictionary<string, List<Package>> pendingByLocation, double capacity)
    {
        string? bestKey = null;
        double maxFitWeight = -1;
        int maxFitCount = -1;

        foreach (var (loc, pkgs) in pendingByLocation)
        {
            var fitting = pkgs.Where(p => p.Weight <= capacity).ToList();
            if (fitting.Count == 0) continue;

            double totalFitWeight = fitting.Sum(p => p.Weight);
            int count = fitting.Count;

            // Prioritize locations with higher fit weight, breaking ties with higher package count
            if (totalFitWeight > maxFitWeight || (Math.Abs(totalFitWeight - maxFitWeight) < 0.001 && count > maxFitCount))
            {
                maxFitWeight = totalFitWeight;
                maxFitCount = count;
                bestKey = loc;
            }
        }

        return bestKey;
    }
}
