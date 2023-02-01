using CORE.Domain.Model;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Models
{
    public class DTripPlan
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        //[BsonElement("_id")]
        public string Id { get; set; }

        public DTripPlan(List<MDrone> droneSorted, DateTime created)
        {
            this.drones = droneSorted;
            this.CreatedAt = created;
        }


        public List<MDrone> drones { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
