using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShortUrl.Models
{
    public sealed class UrlManagement
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string DestinationUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; }
        public DateTime? LastAccessedOn { get; set; }
    }
    
}
