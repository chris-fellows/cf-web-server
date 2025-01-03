using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Handles web request
    /// </summary>
    internal interface IWebRequestHandler
    {
        /// <summary>
        /// Whether instance can handle request
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        bool CanHandle(RequestContext requestContext);

        /// <summary>
        /// Handles request
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        Task HandleAsync(RequestContext requestContext);
    }
}
