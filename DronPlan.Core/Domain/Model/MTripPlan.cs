namespace CORE.Domain.Model
{
    public class MTripPlan 
    {
        public MTripPlan(List<MDrone> droneSorted, DateTime created) {
            this.drones = droneSorted;
            this.CreatedAt = created;
        }
        public MTripPlan(List<MDrone> droneSorted)
        {
            this.drones = droneSorted;
            this.CreatedAt = DateTime.Now;
        }

        public virtual string Id { get; set; }
        public List<MDrone> drones { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
