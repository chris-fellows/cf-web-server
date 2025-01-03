using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Log writer
    /// </summary>
    internal interface ILogWriter
    {
        void Log(string message);

        void LogRequest(RequestContext requestContext);

        void LogResponse(RequestContext requestContext);

    }
}
