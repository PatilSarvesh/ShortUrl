namespace ShortUrl.Models
{
    public sealed record ShortenUrlRequest(
        string? Url,
        string? CustomCode,
        int? ExpirationHours);

    public sealed record ShortenUrlResponse(
        string ShortUrl,
        string ShortCode,
        string DestinationUrl,
        DateTime CreatedOn,
        DateTime? ExpiresAt,
        int ClickCount);

    public sealed record ShortUrlStatsResponse(
        string ShortUrl,
        string ShortCode,
        string DestinationUrl,
        DateTime CreatedOn,
        DateTime? ExpiresAt,
        int ClickCount,
        DateTime? LastAccessedOn,
        bool IsExpired);

    public sealed record UrlResolution(string? DestinationUrl, bool IsExpired);
}
