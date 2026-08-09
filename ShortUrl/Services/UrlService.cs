using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShortUrl.Models;

namespace ShortUrl.Services
{
    public class UrlService : IUrlService
    {
        private readonly IMongoCollection<UrlManagement> _urlCollection;

        public UrlService(IMongoClient mongoClient, IOptions<DatabaseSettings> dbSettings, IOptions<DbCollections> options)
        {
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _urlCollection = mongoDatabase.GetCollection<UrlManagement>(options.Value.UrlCollection);
        }

        public async Task<bool> TrySaveShortUrl(UrlManagement url, CancellationToken cancellationToken = default)
        {
            try
            {
                await _urlCollection.InsertOneAsync(url, cancellationToken: cancellationToken);
                return true;
            }
            catch (MongoWriteException exception)
                when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        public async Task<UrlManagement?> GetUrlByShortCode(string shortCode, CancellationToken cancellationToken = default)
        {
            return await _urlCollection
                .Find(u => u.ShortCode == shortCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task RecordClick(string shortCode, CancellationToken cancellationToken = default)
        {
            var update = Builders<UrlManagement>.Update
                .Inc(url => url.ClickCount, 1)
                .Set(url => url.LastAccessedOn, DateTime.UtcNow);

            await _urlCollection.UpdateOneAsync(
                url => url.ShortCode == shortCode,
                update,
                cancellationToken: cancellationToken);
        }

    }
}
