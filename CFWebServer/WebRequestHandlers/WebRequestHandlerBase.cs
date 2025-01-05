using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Web request handler base class
    /// </summary>
    internal abstract class WebRequestHandlerBase
    {
        protected readonly IFileCacheService _fileCacheService;
        protected readonly ServerData _serverData;

        public WebRequestHandlerBase(IFileCacheService fileCacheService, ServerData serverData)
        {
            _fileCacheService = fileCacheService;
            _serverData = serverData;
        } 
        
        /// <summary>
        /// Whether cache file is not latest
        /// </summary>
        /// <param name="cacheFile"></param>
        /// <returns></returns>
        protected bool IsCacheFileNotTheLatest(CacheFile cacheFile)
        {
            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.RootFolder, cacheFile.RelativePath);

            if (File.Exists(localResourcePath))
            {
                var lastModified = File.GetLastWriteTimeUtc(localResourcePath);
                return lastModified == cacheFile.LastModified;
            }

            return true;
        }

        /// <summary>
        /// Gets resource local path
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        protected string GetResourceLocalPath(string relativePath)
        {
            if (relativePath == "/")
            {
                relativePath = $"/{_serverData.DefaultFile}";
            }

            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.RootFolder, relativePath);

            return localResourcePath;
        }
    }
}
