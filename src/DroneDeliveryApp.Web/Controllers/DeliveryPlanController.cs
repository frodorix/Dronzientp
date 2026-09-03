using System.Text;
using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DroneDeliveryApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/delivery")]
public class DeliveryPlanController : ControllerBase
{
    private readonly ICsvParserService _csvParserService;
    private readonly IDeliveryPlanner _deliveryPlanner;
    private readonly IDeliveryPlanRepository _repository;
    private readonly ILogger<DeliveryPlanController> _logger;

    public DeliveryPlanController(
        ICsvParserService csvParserService,
        IDeliveryPlanner deliveryPlanner,
        IDeliveryPlanRepository repository,
        ILogger<DeliveryPlanController> logger)
    {
        _csvParserService = csvParserService;
        _deliveryPlanner = deliveryPlanner;
        _repository = repository;
        _logger = logger;
    }

    [HttpPost("plan")]
    [RequestSizeLimit(100 * 1024 * 1024)] // Support up to 100MB CSV files
    public async Task<IActionResult> CreatePlan(IFormFile? file, [FromForm] string? rawCsvContent, CancellationToken cancellationToken)
    {
        try
        {
            string fileName = "delivery_input.csv";
            List<Drone> drones;
            List<Package> packages;

            if (file != null && file.Length > 0)
            {
                fileName = file.FileName;
                using var stream = file.OpenReadStream();
                (drones, packages) = _csvParserService.ParseCsv(stream);
            }
            else if (!string.IsNullOrWhiteSpace(rawCsvContent))
            {
                (drones, packages) = _csvParserService.ParseCsv(rawCsvContent);
            }
            else
            {
                return BadRequest(new { message = "No CSV file or CSV content provided." });
            }

            if (drones.Count == 0 && packages.Count == 0)
            {
                return BadRequest(new { message = "Could not parse any valid Drones or Packages from the provided CSV file." });
            }

            _logger.LogInformation("Parsed {DroneCount} drones and {PackageCount} packages from {FileName}", drones.Count, packages.Count, fileName);

            var plan = _deliveryPlanner.CreateDeliveryPlan(drones, packages, fileName);

            var savedPlan = await _repository.SaveAsync(plan, cancellationToken);

            return Ok(savedPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating delivery plan");
            return StatusCode(500, new { message = "An error occurred while creating the delivery plan.", error = ex.Message });
        }
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetRecentPlans([FromQuery] int count = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var plans = await _repository.GetRecentPlansAsync(Math.Min(count, 100), cancellationToken);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent delivery plans");
            return StatusCode(500, new { message = "Failed to retrieve recent plans.", error = ex.Message });
        }
    }

    [HttpGet("plans/{id}")]
    public async Task<IActionResult> GetPlanById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _repository.GetByIdAsync(id, cancellationToken);
            if (plan == null)
            {
                return NotFound(new { message = $"Plan with ID '{id}' was not found." });
            }

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching plan '{Id}'", id);
            return StatusCode(500, new { message = "Failed to retrieve plan.", error = ex.Message });
        }
    }

    [HttpDelete("plans/{id}")]
    public async Task<IActionResult> DeletePlan(string id, CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await _repository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { message = $"Plan with ID '{id}' was not found or already deleted." });
            }

            return Ok(new { message = "Plan deleted successfully.", id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plan '{Id}'", id);
            return StatusCode(500, new { message = "Failed to delete plan.", error = ex.Message });
        }
    }

    [HttpGet("sample-csv")]
    public IActionResult DownloadSampleCsv([FromQuery] string format = "standard")
    {
        string csvContent;
        string fileName;

        if (format.Equals("tabular", StringComparison.OrdinalIgnoreCase))
        {
            csvContent = "Type,Name,Weight,Location\n" +
                         "Drone,Drone A,200,\n" +
                         "Drone,Drone B,250,\n" +
                         "Drone,Drone C,100,\n" +
                         "Package,Pkg-1,200,Location A\n" +
                         "Package,Pkg-2,150,Location B\n" +
                         "Package,Pkg-3,50,Location B\n" +
                         "Package,Pkg-4,150,Location D\n" +
                         "Package,Pkg-5,100,Location E\n" +
                         "Package,Pkg-6,10,Location A\n" +
                         "Package,Pkg-7,50,Location G\n" +
                         "Package,Pkg-8,80,Location H\n" +
                         "Package,Pkg-9,30,Location E\n" +
                         "Package,Pkg-10,50,Location A\n" +
                         "Package,Pkg-11,70,Location E\n" +
                         "Package,Pkg-12,20,Location A\n" +
                         "Package,Pkg-13,50,Location E\n" +
                         "Package,Pkg-14,30,Location A\n" +
                         "Package,Pkg-15,20,Location A\n" +
                         "Package,Pkg-16,10,Location A\n";
            fileName = "sample_tabular_delivery.csv";
        }
        else
        {
            csvContent = "DroneA, 200, DroneB, 250, DroneC, 100\n" +
                         "LocationA, 200\n" +
                         "LocationB, 150\n" +
                         "LocationB, 50\n" +
                         "LocationD, 150\n" +
                         "LocationE, 100\n" +
                         "LocationA, 10\n" +
                         "LocationG, 50\n" +
                         "LocationH, 80\n" +
                         "LocationE, 30\n" +
                         "LocationA, 50\n" +
                         "LocationE, 70\n" +
                         "LocationA, 20\n" +
                         "LocationE, 50\n" +
                         "LocationA, 30\n" +
                         "LocationA, 20\n" +
                         "LocationA, 10\n";
            fileName = "sample_drone_delivery.csv";
        }

        var bytes = Encoding.UTF8.GetBytes(csvContent);
        return File(bytes, "text/csv", fileName);
    }
}
