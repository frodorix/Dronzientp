namespace CORE.Domain.Model
{
    /// <summary>
    /// Represents a package to be delivered by a drone.
    /// </summary>
    public class MPackage  
    {
        /// <summary>
        /// Gets or sets the unique identifier of the package.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the delivery location of the package.
        /// </summary>
        public required string Location { get; set; }
        
        /// <summary>
        /// Gets or sets the weight of the package in the specified unit.
        /// </summary>
        public double Weight { get; set; }
        
        /// <summary>
        /// Gets or sets whether the package has been selected for a delivery trip.
        /// </summary>
        public bool Selected { get; set; }
    }
}
