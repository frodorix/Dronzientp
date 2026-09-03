using DroneDeliveryApp.Core.Models;
using DroneDeliveryApp.Core.Services;
using FluentAssertions;
using Xunit;

namespace DroneDeliveryApp.Tests;

public class DeliveryPlannerTests
{
    private readonly DeliveryPlanner _planner = new();

    [Fact]
    public void ShouldPrioritizeDronesWithLargestCapacityFirst()
    {
        // Arrange
        var drones = new List<Drone>
        {
            new("SmallDrone", 50),
            new("BigDrone", 200),
            new("MediumDrone", 100)
        };

        var packages = new List<Package>
        {
            new("LocationA", 180),
            new("LocationB", 90)
        };

        // Act
        var plan = _planner.CreateDeliveryPlan(drones, packages);

        // Assert
        plan.Should().NotBeNull();
        plan.DronePlans.Should().NotBeEmpty();
        
        // BigDrone (200 capacity) should be used first for the 180kg package
        var bigDronePlan = plan.DronePlans.FirstOrDefault(dp => dp.DroneName == "BigDrone");
        bigDronePlan.Should().NotBeNull();
        bigDronePlan!.Trips.Should().NotBeEmpty();
        bigDronePlan.Trips[0].Packages.Should().Contain(p => p.Location == "LocationA" && p.Weight == 180);
    }

    [Fact]
    public void ShouldGroupPackagesGoingToSameLocationTogether()
    {
        // Arrange
        var drones = new List<Drone>
        {
            new("DroneA", 100)
        };

        var packages = new List<Package>
        {
            new("LocationA", 40),
            new("LocationA", 30),
            new("LocationB", 50)
        };

        // Act
        var plan = _planner.CreateDeliveryPlan(drones, packages);

        // Assert
        plan.DronePlans.Should().HaveCount(1);
        var dronePlan = plan.DronePlans[0];
        
        // Trip 1 should combine LocationA packages (40 + 30 = 70 <= 100)
        var trip1 = dronePlan.Trips[0];
        trip1.PrimaryLocation.Should().Be("LocationA");
        trip1.Packages.Should().HaveCount(2);
        trip1.Packages.Should().OnlyContain(p => p.Location == "LocationA");
    }

    [Fact]
    public void ShouldMarkPackagesExceedingMaxCapacityAsUnassigned()
    {
        // Arrange
        var drones = new List<Drone>
        {
            new("DroneA", 100),
            new("DroneB", 150)
        };

        var packages = new List<Package>
        {
            new("LocationA", 50),
            new("LocationHeavy", 300) // Exceeds 150 max drone capacity
        };

        // Act
        var plan = _planner.CreateDeliveryPlan(drones, packages);

        // Assert
        plan.DeliveredPackagesCount.Should().Be(1);
        plan.UnassignedPackages.Should().HaveCount(1);
        plan.UnassignedPackages[0].Package.Location.Should().Be("LocationHeavy");
        plan.UnassignedPackages[0].Reason.Should().Contain("exceeds maximum available drone capacity");
    }

    [Fact]
    public void ShouldHandleMultipleTripsForSingleDrone()
    {
        // Arrange
        var drones = new List<Drone>
        {
            new("SoloDrone", 100)
        };

        var packages = new List<Package>
        {
            new("Loc1", 80),
            new("Loc2", 80),
            new("Loc3", 80)
        };

        // Act
        var plan = _planner.CreateDeliveryPlan(drones, packages);

        // Assert
        plan.DeliveredPackagesCount.Should().Be(3);
        plan.DronePlans.Should().HaveCount(1);
        plan.DronePlans[0].Trips.Should().HaveCount(3);
    }

    [Fact]
    public void ShouldHandleEmptyInputsGracefully()
    {
        // Arrange & Act
        var plan1 = _planner.CreateDeliveryPlan(new List<Drone>(), new List<Package>());
        var plan2 = _planner.CreateDeliveryPlan(new List<Drone> { new("Drone1", 100) }, new List<Package>());

        // Assert
        plan1.DeliveredPackagesCount.Should().Be(0);
        plan2.DeliveredPackagesCount.Should().Be(0);
    }

    [Fact]
    public void ShouldHandleLargeDatasetEfficiently()
    {
        // Arrange: Generate 5,000 packages and 10 drones
        var drones = Enumerable.Range(1, 10)
            .Select(i => new Drone($"Drone-{i}", 100 + (i * 50))) // Capacities: 150..600
            .ToList();

        var random = new Random(42);
        var locations = new[] { "Hub-Alpha", "Hub-Beta", "Hub-Gamma", "Hub-Delta", "Hub-Epsilon" };
        var packages = Enumerable.Range(1, 5000)
            .Select(i => new Package(locations[random.Next(locations.Length)], random.Next(5, 120), $"Pkg-{i}"))
            .ToList();

        // Act
        var plan = _planner.CreateDeliveryPlan(drones, packages);

        // Assert
        plan.TotalPackages.Should().Be(5000);
        plan.DeliveredPackagesCount.Should().Be(5000);
        plan.ExecutionTimeMs.Should().BeLessThan(2000); // Must complete within 2 seconds
    }
}
