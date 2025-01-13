namespace CFWebServer.Models
{
    /// <summary>
    /// Rule for authorization
    /// </summary>
    public class AuthorizationRule
    {
        /// <summary>
        /// HTTP header where key must be set. E.g. "Authorization"
        /// </summary>
        public string HeaderName { get; set; } = String.Empty;

        /// <summary>
        /// Authorization scheme
        /// </summary>
        public string Scheme { get; set; } = String.Empty;

        /// <summary>
        /// Required value. Type depends on Scheme
        /// </summary>
        public object? Value { get; set; } = null;
    }
}
