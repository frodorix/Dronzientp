using CORE.Application.Interfaces;
using CORE.Application.PlanningAlgorithm;
using CORE.Domain.PlanningAlgorithm;
using DronPlan.Core.Application;
using Microsoft.Extensions.DependencyInjection;
namespace Core.Extensions
{
    public static class ConfigureCore
    {
        public static IServiceCollection UseCoreServices(this IServiceCollection services)
        {
            services.AddTransient<IPlanService, PlanService>();
            services.AddTransient<IPlanningAlgorithm, CustomGreedyAlgorithm>();
            
            return services;
        }
        

    }
}
