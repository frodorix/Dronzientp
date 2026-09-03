using DroneDeliveryApp.Core.Services;
using FluentAssertions;
using Xunit;

namespace DroneDeliveryApp.Tests;

public class CsvParserServiceTests
{
    private readonly CsvParserService _parser = new();

    [Fact]
    public void ShouldParseStandardBracketedCsvFormat()
    {
        // Arrange
        string csv = @"
DroneA, 200, DroneB, 250, DroneC, 100
LocationA, 200
LocationB, 150
LocationC, 50
";

        // Act
        var (drones, packages) = _parser.ParseCsv(csv);

        // Assert
        drones.Should().HaveCount(3);
        drones[0].Name.Should().Be("DroneA");
        drones[0].Capacity.Should().Be(200);

        packages.Should().HaveCount(3);
        packages[0].Location.Should().Be("LocationA");
        packages[0].Weight.Should().Be(200);
    }

    [Fact]
    public void ShouldParseTabularCsvFormatWithHeaders()
    {
        // Arrange
        string csv = @"
Type,Name,Weight,Location
Drone,Drone Alpha,500,
Package,Package 1,120,Location X
Package,Package 2,80,Location Y
";

        // Act
        var (drones, packages) = _parser.ParseCsv(csv);

        // Assert
        drones.Should().HaveCount(1);
        drones[0].Name.Should().Be("Drone Alpha");
        drones[0].Capacity.Should().Be(500);

        packages.Should().HaveCount(2);
        packages[0].Location.Should().Be("Location X");
        packages[0].Weight.Should().Be(120);
    }

    [Fact]
    public void ShouldCleanBracketsAndQuotes()
    {
        // Arrange
        string csv = @"
[Drone 1], [150], [Drone 2], [300]
[Location 10], [45]
";

        // Act
        var (drones, packages) = _parser.ParseCsv(csv);

        // Assert
        drones.Should().HaveCount(2);
        drones[0].Name.Should().Be("Drone 1");
        drones[0].Capacity.Should().Be(150);

        packages.Should().HaveCount(1);
        packages[0].Location.Should().Be("Location 10");
        packages[0].Weight.Should().Be(45);
    }
}
