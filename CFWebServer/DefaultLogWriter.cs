using CFWebServer.Interfaces;
using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer
{
    internal class DefaultLogWriter : ILogWriter
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
