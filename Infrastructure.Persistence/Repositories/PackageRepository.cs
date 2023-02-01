using MongoDB.Driver;
using CORE.Domain.Model;
using CORE.DTO;
using CORE.Interfaces;
using Microsoft.Extensions.Options;
using Infrastructure.Persistence.Models;
using Infrastructure.Persistence.Context;

public class PackageRepository : IPackageRepository
{
    private readonly IMongoCollection<DTripPlan> _plans;

    public PackageRepository(IOptions<MongoSettings> options)
    { 

        var client = new MongoClient(options.Value.ConnectionString);
        var db = client.GetDatabase(options.Value.Database);

        _plans = db.GetCollection<DTripPlan>("TripPlan");
    }

    public async Task<string> createPlan(MTripPlan plan)
    {
        var p = new DTripPlan(plan.drones,plan.CreatedAt);
        
        await _plans.InsertOneAsync(p);
        return plan.Id;
    }

    public async Task<List<MTripPlanDTO>> GetLast10DeliveryPlan()
    {

        var projection = Builders<DTripPlan>.Projection
         .Include(plan => plan.Id)
         .Include(plan => plan.CreatedAt);
         //.Exclude(plan => plan.drones)
        // ;

        var plans = await _plans
         .Find(Builders<DTripPlan>.Filter.Empty)
         .SortByDescending(plan => plan.CreatedAt)
         .Limit(10)
         //.Project<MTripPlanDTO>(projection)
         .ToListAsync();

        return plans.Select(x => new MTripPlanDTO() { Id = x.Id, CreatedAt = x.CreatedAt }).ToList();

        /*
        var projection = Builders<DTripPlan>.Projection
         .Include(plan => plan.Id)
         .Include(plan => plan.CreatedAt)
  
         ;

        var plans = await _plans
            .Find(Builders<DTripPlan>.Filter.Empty)
            .SortByDescending(plan => plan.CreatedAt)
            .Limit(10)
            .Project<MTripPlanDTO>(projection)
            .ToListAsync();

        return plans;*/
    }

    public async Task<MTripPlan> getPlan(string? id)
    {
        var filter = Builders<DTripPlan>.Filter.Eq(x => x.Id, id);
        var project = Builders<DTripPlan>.Projection
            .Include(x => x.Id)
            .Include(x => x.CreatedAt)
            .Include(x => x.drones.Select(y => y.Id))
            .Include(x => x.drones.Select(y => y.Name))
            .Include(x => x.drones.Select(y => y.MaxWeight))
            .Exclude(x => x.drones.Select(y => y.Trips));

        var plan= await _plans.Find(filter).Project<MTripPlan>(project).FirstOrDefaultAsync();


        return plan;

    }


    public async Task<MDrone> getDrone(string planId, int droneId)
    {
        var filter = Builders<DTripPlan>.Filter.And(
            Builders<DTripPlan>.Filter.Eq(x=> x.Id, planId),
            Builders<DTripPlan>.Filter.ElemMatch(x=> x.drones, drone => drone.Id == droneId)
        );


        var projection = Builders<DTripPlan>.Projection.Expression(x => x.drones.Single(drone => drone.Id == droneId));
        var result = await _plans.Find(filter).Project<MDrone>(projection).FirstOrDefaultAsync();
        return result;
        
    }

    
}

