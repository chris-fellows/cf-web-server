using CFWebServer.Constants;
using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles DELETE request for static resource
    /// </summary>
    public class StaticResourceDeleteWebRequestHandler : WebRequestHandlerBase,  IWebRequestHandler
    {        
        public StaticResourceDeleteWebRequestHandler(IFileCacheService fileCacheService,
                                        IMimeTypeDatabase mimeTypeDatabase,
                                        ServerData serverData) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            
        }

        public string Name => WebRequestHandlerNames.StaticResourceDelete;

        //public bool CanHandle(RequestContext requestContext)
        //{
        //    return requestContext.Request.HttpMethod == "DELETE";
        //}

        public async Task HandleAsync(RequestContext requestContext)
        {
            //if (!CanHandle(requestContext))
            //{
            //    throw new ArgumentException("Unable to handle request");
            //}

            var relativePath = requestContext.Request.Url.AbsolutePath;

            var response = requestContext.Response;

            if (IsActionAllowedForFolderPermission(HttpUtilities.GetUrlWithoutLastElement(relativePath), FolderPermissions.Write))
            {
                // Get local path
                var localResourcePath = GetResourceLocalPath(relativePath);                
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
            else
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Close();
            }
        }
    }
}
