using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Site log writer
    /// </summary>
    public interface ISiteLogWriter
    {
        void Log(string message);

        void LogRequest(RequestContext requestContext);        
    }
}
