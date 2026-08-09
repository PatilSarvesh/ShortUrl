using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShortUrl.Models;

namespace ShortUrl.Services
{
    public sealed class MongoIndexInitializer : IHostedService
    {
        private readonly IMongoCollection<UrlManagement> _urlCollection;

        public MongoIndexInitializer(
            IMongoClient mongoClient,
            IOptions<DatabaseSettings> databaseSettings,
            IOptions<DbCollections> collectionSettings)
        {
            var database = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _urlCollection = database.GetCollection<UrlManagement>(collectionSettings.Value.UrlCollection);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var indexKeys = Builders<UrlManagement>.IndexKeys.Ascending(url => url.ShortCode);
            var index = new CreateIndexModel<UrlManagement>(
                indexKeys,
                new CreateIndexOptions { Unique = true, Name = "ShortCode_Unique" });

            await _urlCollection.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
