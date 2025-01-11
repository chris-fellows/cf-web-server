using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServerCommon.Models
{
    /// <summary>
    /// Defines a route rule which maps an incoming request to a web request handler
    /// </summary>
    public class RouteRule
    {
        /// <summary>
        /// HTTP method(s) that route rule is valid for
        /// </summary>
        public List<string> Methods = new List<string>();

        /// <summary>
        /// Relative path(s) that route rule is valid for
        /// </summary>
        public List<string> RelativePaths = new List<string>();

        ///// <summary>
        ///// Valid API keys
        ///// </summary>
        //public List<string> APIKeys = new List<string>();

        /// <summary>
        /// Web request handler to handle request
        /// </summary>
        public string WebRequestHandlerName { get; set; } = String.Empty;
    }
}
