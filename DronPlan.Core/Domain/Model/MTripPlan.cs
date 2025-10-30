namespace CORE.Domain.Model
{
    /// <summary>
    /// Represents a complete delivery trip plan including all drones and their assigned packages.
    /// </summary>
    public class MTripPlan 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MTripPlan"/> class with drones and creation timestamp.
        /// </summary>
        /// <param name="droneSorted">List of drones sorted by their capacity</param>
        /// <param name="created">Timestamp when the plan was created</param>
        public MTripPlan(List<MDrone> droneSorted, DateTime created) 
        {
            this.drones = droneSorted;
            this.CreatedAt = created;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MTripPlan"/> class with drones and current timestamp.
        /// </summary>
        /// <param name="droneSorted">List of drones sorted by their capacity</param>
        public MTripPlan(List<MDrone> droneSorted)
        {
            this.drones = droneSorted;
            this.CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the trip plan.
        /// </summary>
        public virtual string Id { get; set; } = string.Empty;
        
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
