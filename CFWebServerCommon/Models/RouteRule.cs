namespace CFWebServer.Models
{
    /// <summary>
    /// Defines a route rule which maps an incoming request to a web request handler
    /// </summary>
    public class RouteRule : ICloneable
    {
        /// <summary>
        /// HTTP method(s) that route rule is valid for
        /// </summary>
        public List<string> Methods = new List<string>();

        /// <summary>
        /// Relative path patterns that route rule is valid for. May contain wildcards.
        /// 
        /// E.g. "/Folder1/Folder2/*" or "/Folder1/*/Folder3"
        /// 
        /// Please read limits in HttpUtilities.GetRelativeUrlElements
        /// </summary>
        public List<string> RelativePathPatterns = new List<string>();

        /// <summary>
        /// Authorization rules
        /// </summary>
        public List<string> AuthorizationRuleIds = new List<string>();

        /// <summary>
        /// Web request handler to handle request
        /// </summary>
        public string WebRequestHandlerName { get; set; } = String.Empty;

        public object Clone()
        {
            var routeRule = new RouteRule()
            {
                Methods = new List<string>(Methods),
                RelativePathPatterns = new List<string>(RelativePathPatterns),
                AuthorizationRuleIds = new List<string>(AuthorizationRuleIds),
                WebRequestHandlerName = WebRequestHandlerName
            };           
            return routeRule;
        }
    }
}
