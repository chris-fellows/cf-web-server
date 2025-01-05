using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles DELETE request for static resource
    /// </summary>
    internal class StaticResourceDeleteWebRequestHandler : WebRequestHandlerBase,  IWebRequestHandler
    {        
        public StaticResourceDeleteWebRequestHandler(IFileCacheService fileCacheService,
                                        ServerData serverData) : base(fileCacheService, serverData)
        {
            
        }

        public bool CanHandle(RequestContext requestContext)
        {
            return requestContext.Request.HttpMethod == "DELETE";
        }

        public async Task HandleAsync(RequestContext requestContext)
        {
            if (!CanHandle(requestContext))
            {
                throw new ArgumentException("Unable to handle request");
            }

            var relativePath = requestContext.Request.Url.AbsolutePath;

            // Get local path
            var localResourcePath = GetResourceLocalPath(relativePath);
            
            var response = requestContext.Response;
            if (File.Exists(localResourcePath))
            {
                File.Delete(localResourcePath);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            response.Close();

            // Remove cache file if exists
            var cacheFile = _fileCacheService.Get(relativePath);
            if (cacheFile != null)
            {
                _fileCacheService.Remove(relativePath);
            }
        }
    }
}
