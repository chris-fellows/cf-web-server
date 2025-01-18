using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using CFWebServer.Utilities;

namespace CFWebServer.WebRequestHandlers
{
    /// <summary>
    /// Web request handler base class
    /// </summary>
    public abstract class WebRequestHandlerBase
    {
        protected readonly IFileCacheService _fileCacheService;
        protected readonly IMimeTypeDatabase _mimeTypeDatabase;
        protected readonly SiteData _siteData;

        public WebRequestHandlerBase(IFileCacheService fileCacheService, IMimeTypeDatabase mimeTypeDatabase, SiteData siteData)
        {
            _fileCacheService = fileCacheService;
            _mimeTypeDatabase = mimeTypeDatabase;
            _siteData = siteData;
        }
        
        /// <summary>
        /// Whether cache file is not latest
        /// </summary>
        /// <param name="cacheFile"></param>
        /// <returns></returns>
        protected bool IsCacheFileNotTheLatest(CacheFile cacheFile)
        {
            var localResourcePath = HttpUtilities.GetResourceLocalPath(_siteData.SiteConfig.RootFolder, cacheFile.RelativePath);

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
                relativePath = $"/{_siteData.SiteConfig.DefaultFile}";
            }

            var localResourcePath = HttpUtilities.GetResourceLocalPath(_siteData.SiteConfig.RootFolder, relativePath);

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
            var folderConfig = _siteData.SiteConfig.FolderConfigs.FirstOrDefault(fc => fc.RelativePath == relativePath);
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
