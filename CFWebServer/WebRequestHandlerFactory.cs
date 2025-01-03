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
                new StaticResourceDeleteWebRequestHandler(_serverData),
                new StaticResourceGetWebRequestHandler(_serverData),
                new StaticResourcePostWebRequestHandler(_serverData),
                new StaticResourcePutWebRequestHandler(_serverData)
            };

            return list;
        }

        public IWebRequestHandler? Get(RequestContext requestContext)
        {
            // Get web request handlers than can handle this request
            var webRequestHandlers = GetAll().Where(h => h.CanHandle(requestContext)).ToList();

            // Return first
            // TODO: Pick best one
            return webRequestHandlers.FirstOrDefault();
        }
    }
}
