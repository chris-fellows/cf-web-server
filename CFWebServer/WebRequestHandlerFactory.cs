using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebRequestHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer
{
    internal class WebRequestHandlerFactory : IWebRequestHandlerFactory
    {
        private readonly IFolderConfigService _folderConfigService;
        private readonly ServerData _serverData;

        public WebRequestHandlerFactory(IFolderConfigService folderConfigService,
                            ServerData serverData)
        {
            _folderConfigService = folderConfigService;
            _serverData = serverData;
        }

        private List<IWebRequestHandler> GetAll()
        {
            // TODO: Use DI or reflection
            var list = new List<IWebRequestHandler>()
            {
                new StaticResourceWebRequestHandler(_serverData)
            };

            return list;
        }

        public IWebRequestHandler? Get(RequestContext requestContext)
        {
            return GetAll().FirstOrDefault(h => h.CanHandle(requestContext));
        }
    }
}
