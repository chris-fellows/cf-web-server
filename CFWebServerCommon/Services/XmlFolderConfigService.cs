using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.Services
{
    public class XmlFolderConfigService : XmlEntityWithIdStoreService<FolderConfig, string>, IFolderConfigService
    {
        public XmlFolderConfigService(string folder) : base(folder,
                                            "FolderConfig.*.xml",
                                            (gitConfig) => $"FolderConfig.{gitConfig.Id}.xml",
                                            (gitConfigId) => $"FolderConfig.{gitConfigId}.xml")
        {

        }
    }
}
