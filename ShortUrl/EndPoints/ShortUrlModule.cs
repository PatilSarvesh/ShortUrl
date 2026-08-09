using Carter;
using ShortUrl.Factories;
using ShortUrl.Models;

namespace ShortUrl.EndPoints
{
    public sealed class ShortUrlModule : CarterModule
    {
        private const int MaxExpirationHours = 24 * 30;
        private const int MinCustomCodeLength = 4;
        private const int MaxCustomCodeLength = 32;

        public override void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/url", (
                IUrlFactory urlFactory,
                ShortenUrlRequest request,
                CancellationToken cancellationToken) =>
                CreateShortUrlAsync(urlFactory, request, cancellationToken));

            app.MapPost("/ShortenUrl", (
                IUrlFactory urlFactory,
                ShortenUrlRequest request,
                CancellationToken cancellationToken) =>
                CreateShortUrlAsync(urlFactory, request, cancellationToken));

            app.MapGet("/api/url/{shortCode}/stats", async (
                IUrlFactory urlFactory,
                string shortCode,
                CancellationToken cancellationToken) =>
            {
                var stats = await urlFactory.GetStatsAsync(shortCode, cancellationToken);
                return stats is null
                    ? Results.NotFound(new { message = "Short URL not found." })
                    : Results.Ok(stats);
            });

            app.MapGet("/{shortCode}", async (
                IUrlFactory urlFactory,
                string shortCode,
                CancellationToken cancellationToken) =>
            {
                var resolution = await urlFactory.GetDestinationUrl(shortCode, cancellationToken);
                if (resolution.IsExpired)
                {
                    return Results.StatusCode(StatusCodes.Status410Gone);
                }

                return resolution.DestinationUrl is null
                    ? Results.NotFound(new { message = "Short URL not found." })
                    : Results.Redirect(resolution.DestinationUrl);
            });
        }

        private static async Task<IResult> CreateShortUrlAsync(
            IUrlFactory urlFactory,
            ShortenUrlRequest request,
            CancellationToken cancellationToken)
        {
            var destinationUrl = request.Url?.Trim();
            if (!IsHttpUrl(destinationUrl, out var validatedUrl))
            {
                return Results.BadRequest(new
                {
                    message = "Url must be a valid absolute HTTP or HTTPS URL."
                });
            }

            if (!TryValidateCustomCode(request.CustomCode, out var customCode))
            {
                return Results.BadRequest(new
                {
                    message = $"Custom code must be {MinCustomCodeLength}-{MaxCustomCodeLength} characters using letters, numbers, '-' or '_'."
                });
            }

            if (request.ExpirationHours is not null &&
                (request.ExpirationHours < 1 || request.ExpirationHours > MaxExpirationHours))
            {
                return Results.BadRequest(new
                {
                    message = $"Expiration must be between 1 hour and {MaxExpirationHours / 24} days."
                });
            }

            try
            {
                var result = await urlFactory.GenerateShortenUrlAsync(
                    validatedUrl,
                    customCode,
                    request.ExpirationHours,
                    cancellationToken);

                return Results.Ok(result);
            }
            catch (ShortCodeAlreadyExistsException exception)
            {
                return Results.Conflict(new { message = exception.Message });
            }
        }

        private static bool IsHttpUrl(string? value, out string validatedUrl)
        {
            validatedUrl = string.Empty;
            if (string.IsNullOrWhiteSpace(value) ||
                !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                string.IsNullOrWhiteSpace(uri.Host))
            {
                return false;
            }

            validatedUrl = uri.AbsoluteUri;
            return true;
        }

        private static bool TryValidateCustomCode(string? value, out string? customCode)
        {
            customCode = value?.Trim();
            if (string.IsNullOrEmpty(customCode))
            {
                customCode = null;
                return true;
            }

            if (customCode.Length < MinCustomCodeLength || customCode.Length > MaxCustomCodeLength)
            {
                return false;
            }

            foreach (var character in customCode)
            {
                if (!char.IsLetterOrDigit(character) && character is not '-' and not '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
