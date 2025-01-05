using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.WebRequestHandlers;

namespace CFWebServer
{
    /// <summary>
    /// Factory for IWebRequestHandler instances
    /// </summary>
    internal class WebRequestHandlerFactory : IWebRequestHandlerFactory
    {
        private readonly ICacheService _cacheService;
        private readonly IFileCacheService _fileCacheService;
        private readonly IFolderConfigService _folderConfigService;
        private readonly ServerData _serverData;

        public WebRequestHandlerFactory(ICacheService cacheService,
                                        IFileCacheService fileCacheService,
                                        IFolderConfigService folderConfigService,
                                        ServerData serverData)
        {
            _cacheService = cacheService;
            _fileCacheService = fileCacheService;
            _folderConfigService = folderConfigService;
            _serverData = serverData;
        }

        private List<IWebRequestHandler> GetAll()
        {
            // TODO: Use DI or reflection
            var list = new List<IWebRequestHandler>()
            {
                //new TestCustomGetWebRequestHandler(_fileCacheService, _serverData),
                new StaticResourceDeleteWebRequestHandler(_fileCacheService, _serverData),
                new StaticResourceGetWebRequestHandler(_fileCacheService, _serverData),
                new StaticResourcePostWebRequestHandler(_fileCacheService, _serverData),
                new StaticResourcePutWebRequestHandler(_fileCacheService, _serverData)
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
