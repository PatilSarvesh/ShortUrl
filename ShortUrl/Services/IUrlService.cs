using ShortUrl.Models;

namespace ShortUrl.Services
{
    public interface IUrlService
    {
         public  Task SaveShortUrl(UrlManagement url);
         public  Task<UrlManagement> GetUrlByShortCode(string shortCode);
         public  Task<UrlManagement> GetDestinationUrl(string shortUrl);
    }
    
}