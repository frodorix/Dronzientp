namespace DroneDeliveryApp.Core.Models;

public class Drone
{
    public string Name { get; set; } = string.Empty;
    public double Capacity { get; set; }

    public Drone() { }

    public Drone(string name, double capacity)
    {
        Name = name;
        Capacity = capacity;
    }
}

public class Package
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Weight { get; set; }

    public Package() { }

    public Package(string location, double weight, string? name = null)
    {
        Location = location;
        Weight = weight;
        Name = name ?? location;
    }
}

public class UnassignedPackage
{
    public Package Package { get; set; } = new();
    public string Reason { get; set; } = string.Empty;

    public UnassignedPackage() { }

    public UnassignedPackage(Package package, string reason)
    {
        Package = package;
        Reason = reason;
    }
}

public class DeliveryTrip
{
    public int TripNumber { get; set; }
    public string DroneName { get; set; } = string.Empty;
    public string PrimaryLocation { get; set; } = string.Empty;
    public List<Package> Packages { get; set; } = new();
    public double TotalWeight => Packages.Sum(p => p.Weight);
    public double Capacity { get; set; }
    public double CapacityUtilizationPercentage => Capacity > 0 ? Math.Round((TotalWeight / Capacity) * 100.0, 1) : 0;
}

public class DronePlan
{
    public string DroneName { get; set; } = string.Empty;
    public double Capacity { get; set; }
    public List<DeliveryTrip> Trips { get; set; } = new();
    public int TotalTrips => Trips.Count;
    public double TotalWeightDelivered => Trips.Sum(t => t.TotalWeight);
}

public class DeliveryPlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FileName { get; set; } = "delivery_input.csv";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int TotalDrones { get; set; }
    public int TotalPackages { get; set; }
    public int DeliveredPackagesCount => DronePlans.Sum(dp => dp.Trips.Sum(t => t.Packages.Count));
    public int TotalTrips => DronePlans.Sum(dp => dp.TotalTrips);
    public double TotalWeightDelivered => DronePlans.Sum(dp => dp.TotalWeightDelivered);
    public double ExecutionTimeMs { get; set; }
    public List<UnassignedPackage> UnassignedPackages { get; set; } = new();
    public List<DronePlan> DronePlans { get; set; } = new();

    public double OverallCapacityUtilizationPercentage
    {
        get
        {
            var totalAssignedCapacity = DronePlans.Sum(dp => dp.Trips.Sum(t => t.Capacity));
            return totalAssignedCapacity > 0 ? Math.Round((TotalWeightDelivered / totalAssignedCapacity) * 100.0, 1) : 0;
        }
    }
}
