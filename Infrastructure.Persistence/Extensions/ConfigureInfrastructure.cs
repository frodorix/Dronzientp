using CORE.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Extensions
{
    public static class ConfigureInfrastructure
    {
        public static IServiceCollection UseInfrastructurePersistence(this IServiceCollection services)
        {
            services.AddTransient<IPackageRepository, PackageRepository>();

            return services;
        }
      

    }
}
