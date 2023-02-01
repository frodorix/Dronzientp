using CORE.Application.Interfaces;
using CORE.Domain.Model;
using CORE.DTO;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class PlanController : Controller
    {
        private readonly IPlanService planService;
    
        public PlanController(  IPlanService planService)
        {
            this.planService = planService;
        }
        public async Task<IActionResult> IndexAsync()
        {
            List<MTripPlanDTO> top10Items = await planService.GetLast10DeliveryPlan();
            return View(top10Items);
        }

        public async Task<IActionResult> DeliveryPlanAsync(string? id)
        {
            MTripPlan dto = await planService.getPlan(id);
            return View(dto);
        }

        public async Task<IActionResult> DroneAsync(string planId, int droneId)
        {
            MDrone dto = await planService.getDrone(planId, droneId);
            return View(dto);
        }



    }
}
