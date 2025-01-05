using CFWebServer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Models
{
    /// <summary>
    /// Folder configuration
    /// </summary>
    public class FolderConfig
    {
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Relative path from root
        /// </summary>
        public string RelativePath { get; set; } = String.Empty;

        /// <summary>
        /// Permissions
        /// </summary>
        public List<FolderPermissions> Permissions { get; set; } = new List<FolderPermissions>();
    }
}
