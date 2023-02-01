using CORE.Application.Interfaces;
using DronZient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace DronZient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlanService planService;
        private IFormFile myFormFile { get; set; }
        public HomeController(ILogger<HomeController> logger, IPlanService planService)
        {
            _logger = logger;
            this.planService = planService;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("/")]
        public async Task<IActionResult> Upload(IFormFile myFile)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", myFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    myFile.CopyTo(stream);

                }


                var textOutput = await this.planService.ImportFile(filePath);

                byte[] fileBytes = Encoding.UTF8.GetBytes(textOutput);
                return File(fileBytes, "text/plain", "TripPlanOutput.txt");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            //return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}