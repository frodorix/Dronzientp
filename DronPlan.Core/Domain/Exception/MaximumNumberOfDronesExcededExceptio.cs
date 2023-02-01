using System.Runtime.Serialization;

namespace CORE.Domain.Exception
{
    [Serializable]
    public class MaximumNumberOfDronesExcededExceptio : IOException
    {
        public MaximumNumberOfDronesExcededExceptio()
        {
        }

        public MaximumNumberOfDronesExcededExceptio(string? message) : base(message)
        {
            
        }



        
    }
}