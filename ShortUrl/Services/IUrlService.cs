using ShortUrl.Models;

namespace ShortUrl.Services
{
    public interface IUrlService
    {
        Task<bool> TrySaveShortUrl(UrlManagement url, CancellationToken cancellationToken = default);
        Task<UrlManagement?> GetUrlByShortCode(string shortCode, CancellationToken cancellationToken = default);
        Task RecordClick(string shortCode, CancellationToken cancellationToken = default);
    }
}
