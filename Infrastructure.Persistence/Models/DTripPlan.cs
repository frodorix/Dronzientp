using CORE.Domain.Model;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Infrastructure.Persistence.Models
{
    /// <summary>
    /// Database model for trip plan storage in MongoDB.
    /// Represents a persisted version of MTripPlan with MongoDB-specific attributes.
    /// </summary>
    public class DTripPlan
    {
        /// <summary>
        /// Gets or sets the MongoDB ObjectId for this trip plan.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DTripPlan"/> class.
        /// </summary>
        /// <param name="droneSorted">List of drones with their assigned trips</param>
        /// <param name="created">Timestamp when the plan was created</param>
        public DTripPlan(List<MDrone> droneSorted, DateTime created)
        {
            this.drones = droneSorted;
            this.CreatedAt = created;
        }

        /// <summary>
        /// Gets or sets the list of drones with their assigned delivery trips.
        /// </summary>
        public List<MDrone> drones { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when this plan was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
