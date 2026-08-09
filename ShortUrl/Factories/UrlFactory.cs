using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using ShortUrl.Common;
using ShortUrl.Models;
using ShortUrl.Services;

namespace ShortUrl.Factories
{
    public sealed class UrlFactory : IUrlFactory
    {
        private const int MaxInsertAttempts = 10;
        private readonly IUrlService _urlService;
        private readonly string _baseUrl;

        public UrlFactory(IUrlService urlService, IOptions<UrlSettings> urlSettings)
        {
            _urlService = urlService;
            _baseUrl = urlSettings.Value.BaseUrl.TrimEnd('/') + "/";
        }

        public async Task<ShortenUrlResponse> GenerateShortenUrlAsync(
            string destinationUrl,
            string? customCode,
            int? expirationHours,
            CancellationToken cancellationToken = default)
        {
            DateTime? expiresAt = expirationHours.HasValue
                ? DateTime.UtcNow.AddHours(expirationHours.Value)
                : null;

            if (!string.IsNullOrWhiteSpace(customCode))
            {
                var customUrl = CreateUrl(destinationUrl, customCode.Trim(), expiresAt);
                if (!await _urlService.TrySaveShortUrl(customUrl, cancellationToken))
                {
                    throw new ShortCodeAlreadyExistsException(customCode);
                }

                return ToResponse(customUrl);
            }

            for (var attempt = 1; attempt <= MaxInsertAttempts; attempt++)
            {
                var url = CreateUrl(destinationUrl, GenerateShortCode(), expiresAt);
                if (await _urlService.TrySaveShortUrl(url, cancellationToken))
                {
                    return ToResponse(url);
                }
            }

            throw new InvalidOperationException("Could not generate a unique short URL.");
        }

        private UrlManagement CreateUrl(string destinationUrl, string shortCode, DateTime? expiresAt)
        {
            return new UrlManagement
            {
                DestinationUrl = destinationUrl,
                ShortCode = shortCode,
                ShortUrl = _baseUrl + shortCode,
                CreatedOn = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };
        }

        private static string GenerateShortCode()
        {
            var codeChars = new char[Constants.ShortCode.Length];
            for (var i = 0; i < codeChars.Length; i++)
            {
                var randomIndex = RandomNumberGenerator.GetInt32(Constants.ShortCode.Characters.Length);
                codeChars[i] = Constants.ShortCode.Characters[randomIndex];
            }

            return new string(codeChars);
        }

        public async Task<UrlResolution> GetDestinationUrl(
            string shortCode,
            CancellationToken cancellationToken = default)
        {
            var url = await _urlService.GetUrlByShortCode(shortCode, cancellationToken);
            if (url is null)
            {
                return new UrlResolution(null, false);
            }

            if (url.ExpiresAt.HasValue && url.ExpiresAt.Value <= DateTime.UtcNow)
            {
                return new UrlResolution(null, true);
            }

            await _urlService.RecordClick(shortCode, cancellationToken);
            return new UrlResolution(url.DestinationUrl, false);
        }

        public async Task<ShortUrlStatsResponse?> GetStatsAsync(
            string shortCode,
            CancellationToken cancellationToken = default)
        {
            var url = await _urlService.GetUrlByShortCode(shortCode, cancellationToken);
            if (url is null)
            {
                return null;
            }

            return new ShortUrlStatsResponse(
                url.ShortUrl,
                url.ShortCode,
                url.DestinationUrl,
                url.CreatedOn,
                url.ExpiresAt,
                url.ClickCount,
                url.LastAccessedOn,
                url.ExpiresAt.HasValue && url.ExpiresAt.Value <= DateTime.UtcNow);
        }

        private static ShortenUrlResponse ToResponse(UrlManagement url)
        {
            return new ShortenUrlResponse(
                url.ShortUrl,
                url.ShortCode,
                url.DestinationUrl,
                url.CreatedOn,
                url.ExpiresAt,
                url.ClickCount);
        }
    }
}
