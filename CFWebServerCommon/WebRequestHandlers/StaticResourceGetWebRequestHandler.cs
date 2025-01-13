using CFWebServer.Constants;
using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.Net;
using System.Text;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Handles GET request for static resource
    /// </summary>
    public class StaticResourceGetWebRequestHandler : WebRequestHandlerBase,  IWebRequestHandler
    {        
        public StaticResourceGetWebRequestHandler(IFileCacheService fileCacheService,
                                            IMimeTypeDatabase mimeTypeDatabase,
                                            ServerData serverData) : base(fileCacheService, mimeTypeDatabase, serverData)
        {
            
        }

        public string Name => WebRequestHandlerNames.StaticResourceGet;

        //public bool CanHandle(RequestContext requestContext)
        //{
        //    return requestContext.Request.HttpMethod == "GET";
        //}            

        public async Task HandleAsync(RequestContext requestContext)
        {
            //if (!CanHandle(requestContext))
            //{
            //    throw new ArgumentException("Unable to handle request");
            //}

            var response = requestContext.Response;

            var relativePath = requestContext.Request.Url.AbsolutePath;

            if (IsActionAllowedForFolderPermission(HttpUtilities.GetUrlWithoutLastElement(relativePath), FolderPermissions.Read))
            {
                // Get cache file if exists
                var cacheFile = _fileCacheService.Get(relativePath);

                // Remove cached file if not latest
                if (cacheFile != null && IsCacheFileNotTheLatest(cacheFile))
                {
                    _fileCacheService.Remove(relativePath);
                    cacheFile = null;
                }

                // Get mime type info
                var mimeTypeInfo = _mimeTypeDatabase.GetByFileExtension(HttpUtilities.GetUrlFileExtension(relativePath)).FirstOrDefault();

                if (cacheFile == null)    // File not cached
                {
                    // Getlocal path
                    var localResourcePath = GetResourceLocalPath(relativePath);

                    if (File.Exists(localResourcePath))
                    {
                        var content = File.ReadAllBytes(localResourcePath);

                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentType = mimeTypeInfo == null ? "" : mimeTypeInfo.MimeType;
                        response.ContentEncoding = Encoding.UTF8;
                        response.ContentLength64 = content.LongLength;

                        await response.OutputStream.WriteAsync(content, 0, content.Length);

                        // Cache file                    
                        _fileCacheService.Add(relativePath, content, new FileInfo(localResourcePath).LastWriteTimeUtc);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else    // File cached
                {
                    response.StatusCode = (int)HttpStatusCode.OK;

                    var content = cacheFile.GetContent();

                    response.ContentType = mimeTypeInfo == null ? "" : mimeTypeInfo.MimeType;
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentLength64 = content.LongLength;

                    await response.OutputStream.WriteAsync(content, 0, content.Length);

                    _fileCacheService.UpdateLastUsed(relativePath, DateTimeOffset.UtcNow);
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;                
            }
            response.Close();
        }        
    }
}
