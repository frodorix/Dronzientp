namespace CORE.Domain.Model
{
    public class MDrone 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxWeight { get; set; }

        public List<List<MPackage>> Trips { get; set; }
    }
}
