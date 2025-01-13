namespace CFWebServer.Models
{
    public class MimeTypeInfo
    {
        public string MimeType { get; internal set; }

        public string[] FileExtensions { get; internal set; }
         
        public MimeTypeInfo(string mimeType, string[] fileExtensions)
        {
            MimeType = mimeType;
            FileExtensions = fileExtensions;
        }
    }
}
