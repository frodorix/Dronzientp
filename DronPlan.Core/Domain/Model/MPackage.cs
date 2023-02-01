namespace CORE.Domain.Model
{
    public class MPackage  
    {
        private int v;

        public MPackage()
        {
            this.Weight = 0;
        }

        public int Id { get; set; }
        public string Location { get; set; }
        public int Weight { get; set; }
        public bool Selected { get;  set; }
    }
}
