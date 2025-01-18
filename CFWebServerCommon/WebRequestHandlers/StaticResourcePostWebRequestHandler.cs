using CFWebServer.Constants;
using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles POST request for static resource
    /// </summary>
    public class StaticResourcePostWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {        
        public StaticResourcePostWebRequestHandler(IFileCacheService fileCacheService,
                                                IMimeTypeDatabase mimeTypeDatabase,
                                                SiteData serverData) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            
        }

        public string Name => WebRequestHandlerNames.StaticResourcePost;
        
        public async Task HandleAsync(RequestContext requestContext)
        {          
            var relativePath = requestContext.Request.Url.AbsolutePath;

            // Getlocal path
            var localResourcePath = GetResourceLocalPath(relativePath);

            // Write            
            var request = requestContext.Request;
            var response = requestContext.Response;

            if (IsActionAllowedForFolderPermission(HttpUtilities.GetUrlWithoutLastElement(relativePath), FolderPermissions.Write))
            {
                using (var fileStream = new FileStream(localResourcePath, FileMode.OpenOrCreate))
                {
                    await request.InputStream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();

                // Update cache file if exists
                var cacheFile = _fileCacheService.Enabled ? _fileCacheService.Get(relativePath) : null;
                if (cacheFile != null)
                {
                    _fileCacheService.Add(relativePath, new byte[0], new FileInfo(localResourcePath).LastWriteTimeUtc);
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
