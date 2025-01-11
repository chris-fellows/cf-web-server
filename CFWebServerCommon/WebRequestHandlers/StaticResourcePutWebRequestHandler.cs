using CFWebServer.Constants;
using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles PUT request for static resource
    /// </summary>
    public class StaticResourcePutWebRequestHandler : WebRequestHandlerBase, IWebRequestHandler
    {        
        public StaticResourcePutWebRequestHandler(IFileCacheService fileCacheService,
                                                 ServerData serverData) : base(fileCacheService, serverData)
        {
            
        }

        public string Name => WebRequestHandlerNames.StaticResourcePut;

        //public bool CanHandle(RequestContext requestContext)
        //{
        //    return requestContext.Request.HttpMethod == "PUT";
        //}

        public async Task HandleAsync(RequestContext requestContext)
        {
            //if (!CanHandle(requestContext))
            //{
            //    throw new ArgumentException("Unable to handle request");
            //}

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
                var cacheFile = _fileCacheService.Get(relativePath);
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
