using CFWebServer.Utilities;

namespace CFWebServer.Models
{
    /// <summary>
    /// Cache file
    /// </summary>
    internal class CacheFile
    {
        /// <summary>
        /// File path
        /// </summary>
        public string RelativePath { get; set; } = String.Empty;

        public DateTimeOffset LastModified { get; set; } = DateTimeOffset.MinValue;           

        /// <summary>
        /// Whether stored as compressed
        /// </summary>
        public bool Compressed { get; internal set; }

        /// <summary>
        /// Time last used (Read/Write)
        /// </summary>
        public DateTimeOffset LastUsed { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Size of content in internal format
        /// </summary>
        public int ObjectSizeBytes
        {
            get
            {
                return _content.Length;
            }
        }

        /// <summary>
        /// File content (Will be compressed if Compressed=true)
        /// </summary>
        private byte[] _content = new byte[0];
        
        /// <summary>
        /// Sets content. Stores compressed if parameter set
        /// </summary>
        /// <param name="content"></param>
        /// <param name="compressed"></param>
        public void SetContent(byte[] content, bool compressed)
        {
            if (content.Length == 0)
            {
                _content = content;
            }
            else if (compressed)
            {
                _content = CompressionUtilities.CompressWithDeflate(content);
            }
            else
            {
                _content = content;
            }            
        }    
        
        /// <summary>
        /// Gets content. Uncompressed if required
        /// </summary>
        /// <returns></returns>
        public byte[] GetContent()
        {
            if (_content.Length == 0)
            {
                return _content;
            }
            else if (Compressed)
            {
                return CompressionUtilities.DecompressWithDeflate(_content);
            }

            return _content;
        }
    }
}
