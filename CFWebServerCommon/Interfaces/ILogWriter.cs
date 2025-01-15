using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Log writer
    /// </summary>
    public interface ILogWriter
    {
        void Log(string message);

        void LogRequest(RequestContext requestContext);

        void LogResponse(RequestContext requestContext);
    }
}
