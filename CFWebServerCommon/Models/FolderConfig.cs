using CFWebServer.Enums;

namespace CFWebServer.Models
{
    /// <summary>
    /// Folder configuration
    /// </summary>
    public class FolderConfig : ICloneable
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Relative path from root
        /// </summary>
        public string RelativePath { get; set; } = String.Empty;

        /// <summary>
        /// Permissions
        /// </summary>
        public List<FolderPermissions> Permissions { get; set; } = new List<FolderPermissions>();

        public object Clone()
        {
            var folderConfig = new FolderConfig()
            {
                Id = Id,
                RelativePath = RelativePath,
                Permissions = new List<FolderPermissions>(Permissions)
            };
            return folderConfig;
        }
    }
}
