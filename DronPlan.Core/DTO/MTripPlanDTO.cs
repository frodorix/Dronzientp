using CORE.Domain.Model;

namespace CORE.DTO
{
    /// <summary>
    /// Data Transfer Object for trip plan summary information.
    /// Contains minimal information for listing trip plans without full drone details.
    /// </summary>
    public class MTripPlanDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the trip plan.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the timestamp when this plan was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
