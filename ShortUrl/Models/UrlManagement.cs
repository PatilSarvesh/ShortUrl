using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShortUrl.Models
{
    public class UrlManagement
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {  get; set; }
        public string DestinationUrl{ get; set; }  
        public string ShortUrl{ get; set; }
        public string ShortCode { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    
}