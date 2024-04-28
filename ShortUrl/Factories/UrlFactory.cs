using ShortUrl.Common;
using ShortUrl.Models;
using ShortUrl.Services;

namespace ShortUrl.Factories
{
    public class UrlFactory : IUrlFactory
    {
        private readonly IUrlService _urlService;
        private readonly Random _random = new();
        public UrlFactory(IUrlService urlService)
        {
            _urlService = urlService;
        }

        public async Task<string> GenerateShortenUrlAsync(string destinationUrl)
        {
            var shortCode = await GeneratShortCode(destinationUrl);

            var url = new UrlManagement()
            {
                DestinationUrl = destinationUrl,
                ShortCode = shortCode,
                ShortUrl = Constants.Url.BaseUrl + shortCode,
                CreatedOn = DateTime.UtcNow
            };

            await _urlService.SaveShortUrl(url);

            return url.ShortUrl;
        }

        private async Task<string> GeneratShortCode(string url)
        {
            var codeChars = new Char[Constants.Encreption.ShortUrlLength];

            while (true)
            {
                for (int i = 0; i < Constants.Encreption.ShortUrlLength; i++)
                {
                    var randomIndex = _random.Next(Constants.Encreption.Charaters.Length - 1);
                    codeChars[i] = Constants.Encreption.Charaters[randomIndex];
                }
                var code = new string(codeChars);

                var codeExist = await _urlService.GetUrlByShortCode(code);

                if (codeExist == null)
                {
                    return code;
                }
            }


        }

        public async Task<string> GetDestinationUrl(string shortUrl)
        {
            var url = await _urlService.GetUrlByShortCode(shortUrl);

            return url == null ? throw new Exception("Url Not Found") : url.DestinationUrl;
        }
    }

}