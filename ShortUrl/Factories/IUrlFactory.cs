using ShortUrl.Models;

namespace ShortUrl.Factories
{
    public interface IUrlFactory
    {
        Task<ShortenUrlResponse> GenerateShortenUrlAsync(
            string destinationUrl,
            string? customCode,
            int? expirationHours,
            CancellationToken cancellationToken = default);

        Task<UrlResolution> GetDestinationUrl(
            string shortCode,
            CancellationToken cancellationToken = default);

        Task<ShortUrlStatsResponse?> GetStatsAsync(
            string shortCode,
            CancellationToken cancellationToken = default);
    }
}
