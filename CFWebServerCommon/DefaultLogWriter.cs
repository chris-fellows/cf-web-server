using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer
{
    public class DefaultLogWriter : ILogWriter
    {
        public void Log(string message)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow.ToString()} {message}");
        }

        public void LogRequest(RequestContext requestContext)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow.ToString()} Request: Method={requestContext.Request.HttpMethod}; " +
                            $"URL={requestContext.Request.Url.ToString()}");
        }

        public void LogResponse(RequestContext requestContext)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow.ToString()} Response: Method={requestContext.Request.HttpMethod}; " +
                            $"URL={requestContext.Request.Url.ToString()}; Status={requestContext.Response.StatusCode}");
        }
    }
}
