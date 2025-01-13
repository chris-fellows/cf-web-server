using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.AuthorizationManagers
{
    /// <summary>
    /// API key authorization manager
    /// </summary>
    public class ApiKeyAuthorizationManager : IAuthorizationManager
    {
        public string Scheme => "Apikey";
        
        public bool IsAuthorized(RequestContext requestContext, AuthorizationRule authorizationRule)
        {
            // Check for any valid authorization rule
            var request = requestContext.Request;

            var headerValue = request.Headers[authorizationRule.HeaderName];

            return headerValue == $"{Scheme} {(string)authorizationRule.Value}";
        }
    }
}
