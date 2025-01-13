using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// MIME type database
    /// </summary>
    public interface IMimeTypeDatabase
    {
        List<MimeTypeInfo> GetAll();

        MimeTypeInfo? GetByMimeType(string mimeType);
        
        List<MimeTypeInfo> GetByFileExtension(string fileExtension);
    }
}
