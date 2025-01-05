using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.Services
{
    public class XmlSiteConfigService : XmlEntityWithIdStoreService<SiteConfig, string>, ISiteConfigService
    {
        public XmlSiteConfigService(string folder) : base(folder,
                                            "SiteConfig.*.xml",
                                            (siteConfig) => $"SiteConfig.{siteConfig.Id}.xml",
                                            (siteConfigId) => $"SiteConfig.{siteConfigId}.xml")
        {

        }
    }
}
