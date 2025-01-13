namespace CFWebServer.Models
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

        /// <summary>
        /// Authorization rules that route rule is valid for
        /// </summary>
        public List<AuthorizationRule>? AuthorizationRules = null;

        /// <summary>
        /// Web request handler to handle request
        /// </summary>
        public string WebRequestHandlerName { get; set; } = String.Empty;
    }
}
