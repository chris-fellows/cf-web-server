using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebRequestHandlers;

namespace CFWebServer
{
    /// <summary>
    /// Factory for IWebRequestHandler instances
    /// </summary>
    public class WebRequestHandlerFactory : IWebRequestHandlerFactory
    {        
        private readonly IFileCacheService _fileCacheService;
        private readonly IFolderConfigService _folderConfigService;        

        public WebRequestHandlerFactory(IFileCacheService fileCacheService,
                                        IFolderConfigService folderConfigService)                                        
        {            
            _fileCacheService = fileCacheService;
            _folderConfigService = folderConfigService;
        }     

        private List<IWebRequestHandler> GetAll(ServerData serverData)
        {
            // TODO: Use DI or reflection
            var list = new List<IWebRequestHandler>()
            {
                //new TestCustomGetWebRequestHandler(_fileCacheService, serverData),
                new StaticResourceDeleteWebRequestHandler(_fileCacheService, serverData),
                new StaticResourceGetWebRequestHandler(_fileCacheService, serverData),
                new StaticResourcePostWebRequestHandler(_fileCacheService, serverData),
                new StaticResourcePutWebRequestHandler(_fileCacheService, serverData)
            };

            return list;
        }

        public IWebRequestHandler? Get(RequestContext requestContext, ServerData serverData)
        {
            // Get web request handlers than can handle this request
            var webRequestHandlers = GetAll(serverData).Where(h => h.CanHandle(requestContext)).ToList();

            // Return first
            // TODO: Pick best one
            return webRequestHandlers.FirstOrDefault();
        }
    }
}
