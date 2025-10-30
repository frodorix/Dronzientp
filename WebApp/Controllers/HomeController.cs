using CORE.Application.Interfaces;
using DronZient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace DronZient.Controllers
{
    /// <summary>
    /// Controller for handling file uploads and delivery plan generation.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlanService _planService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="planService">Service for delivery plan operations</param>
        public HomeController(ILogger<HomeController> logger, IPlanService planService)
        {
            _logger = logger;
            _planService = planService;
        }

        /// <summary>
        /// Displays the main upload page.
        /// </summary>
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Handles file upload, processes the delivery plan, and returns the output file.
        /// </summary>
        /// <param name="myFile">The uploaded CSV file</param>
        /// <returns>A downloadable text file with the delivery plan</returns>
        [HttpPost("/")]
        public async Task<IActionResult> Upload(IFormFile myFile)
        {
            try
            {
                if (myFile == null || myFile.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);

                var filePath = Path.Combine(uploadsDir, myFile.FileName);
                
                // Save uploaded file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await myFile.CopyToAsync(stream);
                }

                // Process the file and generate delivery plan
                var textOutput = await _planService.ImportFile(filePath);

                // Clean up uploaded file
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete uploaded file: {FilePath}", filePath);
                }

                // Return the delivery plan as a downloadable file
                byte[] fileBytes = Encoding.UTF8.GetBytes(textOutput);
                return File(fileBytes, "text/plain", "TripPlanOutput.txt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing upload");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
