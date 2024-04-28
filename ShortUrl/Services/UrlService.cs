using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShortUrl.Models;

namespace ShortUrl.Services
{
    public class UrlService : IUrlService
    {
        private readonly IMongoCollection<UrlManagement> _urlCollection;
        private readonly IMongoDatabase _mongoDatabase;

        public UrlService(IOptions<DatabaseSettings> dbSettings, IOptions<DbCollections> options)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            _mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _urlCollection = _mongoDatabase.GetCollection<UrlManagement>(options.Value.UrlCollection);
        }

        public async Task SaveShortUrl(UrlManagement url)
        {
            await _urlCollection.InsertOneAsync(url);
        }

        public async Task<UrlManagement> GetUrlByShortCode(string shortCode)
        {
            var codeExist = await _urlCollection.Find(u => u.ShortCode == shortCode).FirstOrDefaultAsync();
            return codeExist ?? null;
        }

        public async Task<UrlManagement> GetDestinationUrl(string shortUrl)
        {
            var url = await _urlCollection.Find(u => u.ShortUrl == shortUrl).FirstOrDefaultAsync();
            return url ?? null;
        }

     }
}