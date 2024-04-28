namespace ShortUrl.Factories
{
    public interface IUrlFactory
    {
        public Task<string> GenerateShortenUrlAsync(string destinationUrl);
        public Task<string> GetDestinationUrl(string shortUrl);
    }

}