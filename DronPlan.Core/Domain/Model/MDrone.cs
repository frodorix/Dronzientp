namespace CORE.Domain.Model
{
    /// <summary>
    /// Represents a delivery drone with its specifications and assigned delivery trips.
    /// </summary>
    public class MDrone 
    {
        /// <summary>
        /// Gets or sets the unique identifier of the drone.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the drone.
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum weight capacity of the drone in the specified unit.
        /// </summary>
        public double MaxWeight { get; set; }

        /// <summary>
        /// Gets or sets the list of delivery trips assigned to this drone.
        /// Each trip contains a list of packages to be delivered in a single journey.
        /// </summary>
        public required List<List<MPackage>> Trips { get; set; }
    }
}
