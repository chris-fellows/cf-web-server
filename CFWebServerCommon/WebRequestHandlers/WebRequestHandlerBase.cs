using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;
using System.ComponentModel.DataAnnotations;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Web request handler base class
    /// </summary>
    public abstract class WebRequestHandlerBase
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
            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.SiteConfig.RootFolder, cacheFile.RelativePath);

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
                relativePath = $"/{_serverData.SiteConfig.DefaultFile}";
            }

            var localResourcePath = HttpUtilities.GetResourceLocalPath(_serverData.SiteConfig.RootFolder, relativePath);

            return localResourcePath;
        }

        /// <summary>
        /// Whether action is allowed for required permission
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="requiredFolderPermission"></param>
        /// <returns></returns>
        protected bool IsActionAllowedForFolderPermission(string relativePath, FolderPermissions requiredFolderPermission)
        {
            var folderConfig = _serverData.SiteConfig.FolderConfigs.FirstOrDefault(fc => fc.RelativePath == relativePath);
            if (folderConfig == null)
            {
                var elements = relativePath.Split('/');

                // TODO: Get parent folder config
            }

            if (folderConfig == null)   // No parent folder config (Not even at root)
            {
                return true;
            }
            else
            {
                return folderConfig.Permissions.Contains(requiredFolderPermission);
            }
        }
    }
}
