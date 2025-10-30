using CORE.Interfaces;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Extensions
{
    /// <summary>
    /// Extension methods for configuring Infrastructure services.
    /// </summary>
    public static class ConfigureInfrastructure
    {
        /// <summary>
        /// Adds infrastructure persistence services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection UseInfrastructurePersistence(this IServiceCollection services)
        {
            services.AddTransient<IPackageRepository, PackageRepository>();
            return services;
        }
    }
}
