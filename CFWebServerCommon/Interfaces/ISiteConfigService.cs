using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Service for managing SiteConfig instances
    /// </summary>
    public interface ISiteConfigService : IEntityWithIdStoreService<SiteConfig, string>
    {
    }
}
