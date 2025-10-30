using MongoDB.Driver;
using CORE.Domain.Model;
using CORE.DTO;
using CORE.Interfaces;
using Microsoft.Extensions.Options;
using Infrastructure.Persistence.Models;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// MongoDB implementation of the package repository.
    /// Handles persistence and retrieval of delivery plans.
    /// </summary>
    public class PackageRepository : IPackageRepository
    {
        private readonly IMongoCollection<DTripPlan> _plans;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageRepository"/> class.
        /// </summary>
        /// <param name="options">MongoDB configuration settings</param>
        public PackageRepository(IOptions<MongoSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var db = client.GetDatabase(options.Value.Database);
            _plans = db.GetCollection<DTripPlan>("TripPlan");
        }

        /// <summary>
        /// Creates and persists a new delivery plan to MongoDB.
        /// </summary>
        /// <param name="plan">The trip plan to persist</param>
        /// <returns>The ID of the created plan</returns>
        public async Task<string> createPlan(MTripPlan plan)
        {
            var dbPlan = new DTripPlan(plan.drones, plan.CreatedAt);
            await _plans.InsertOneAsync(dbPlan);
            return plan.Id;
        }

        /// <summary>
        /// Retrieves the last 10 delivery plans from the database, sorted by creation date.
        /// </summary>
        /// <returns>List of trip plan summaries</returns>
        public async Task<List<MTripPlanDTO>> GetLast10DeliveryPlan()
        {
            var plans = await _plans
                .Find(Builders<DTripPlan>.Filter.Empty)
                .SortByDescending(plan => plan.CreatedAt)
                .Limit(10)
                .ToListAsync();

            return plans.Select(x => new MTripPlanDTO 
            { 
                Id = x.Id, 
                CreatedAt = x.CreatedAt 
            }).ToList();
        }

        /// <summary>
        /// Retrieves a specific delivery plan by ID without trip details.
        /// </summary>
        /// <param name="id">The plan ID</param>
        /// <returns>The trip plan, or null if not found</returns>
        public async Task<MTripPlan?> getPlan(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var filter = Builders<DTripPlan>.Filter.Eq(x => x.Id, id);
            var projection = Builders<DTripPlan>.Projection
                .Include(x => x.Id)
                .Include(x => x.CreatedAt)
                .Include(x => x.drones.Select(y => y.Id))
                .Include(x => x.drones.Select(y => y.Name))
                .Include(x => x.drones.Select(y => y.MaxWeight))
                .Exclude(x => x.drones.Select(y => y.Trips));

            var plan = await _plans.Find(filter)
                .Project<MTripPlan>(projection)
                .FirstOrDefaultAsync();

            return plan;
        }

        /// <summary>
        /// Retrieves a specific drone with its trips from a delivery plan.
        /// </summary>
        /// <param name="planId">The plan ID</param>
        /// <param name="droneId">The drone ID</param>
        /// <returns>The drone with its trips, or null if not found</returns>
        public async Task<MDrone?> getDrone(string planId, int droneId)
        {
            var filter = Builders<DTripPlan>.Filter.And(
                Builders<DTripPlan>.Filter.Eq(x => x.Id, planId),
                Builders<DTripPlan>.Filter.ElemMatch(x => x.drones, drone => drone.Id == droneId)
            );

            var projection = Builders<DTripPlan>.Projection
                .Expression(x => x.drones.Single(drone => drone.Id == droneId));
            
            var result = await _plans.Find(filter)
                .Project<MDrone>(projection)
                .FirstOrDefaultAsync();
            
            return result;
        }
    }
}
