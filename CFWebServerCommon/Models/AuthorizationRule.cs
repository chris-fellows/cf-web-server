namespace CFWebServer.Models
{
    /// <summary>
    /// Rule for authorization
    /// </summary>
    public class AuthorizationRule : ICloneable
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// HTTP header where key must be set. E.g. "Authorization"
        /// </summary>
        public string HeaderName { get; set; } = String.Empty;

        /// <summary>
        /// Authorization scheme
        /// </summary>
        public string Scheme { get; set; } = String.Empty;

        /// <summary>
        /// API key for Scheme="Apikey"
        /// </summary>
        public string APIKey { get; set; } = String.Empty;

        /// <summary>
        /// Bearer role for Scheme="Bearer"
        /// </summary>
        public string BearerRole { get; set; } = String.Empty;

        public object Clone()
        {
            var authorizationRule = new AuthorizationRule()
            {
                Id = Id,
                HeaderName = HeaderName,
                Scheme = Scheme,
                APIKey = APIKey,
                BearerRole = BearerRole
            };
            return authorizationRule;
        }
    }
}
