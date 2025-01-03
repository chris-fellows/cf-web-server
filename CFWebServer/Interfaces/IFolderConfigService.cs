using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Interfaces
{
    internal interface IFolderConfigService : IEntityWithIdStoreService<FolderConfig, string>
    {
    }
}
