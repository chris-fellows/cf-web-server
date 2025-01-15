using CFWebServer.Interfaces;
using CFWebServer.Models;

namespace CFWebServer.AuthorizationManagers
{
    /// <summary>
    /// Bearer token (JWT) authorization manager
    /// </summary>
    public class BearerAuthorizationManager : IAuthorizationManager
    {
        public string Scheme => "Bearer";

        public bool IsAuthorized(RequestContext requestContext, AuthorizationRule authorizationRule)
        {
            throw new NotImplementedException();

            //// Check for any valid authorization rule
            //var request = requestContext.Request;

            //var headerValue = request.Headers[authorizationRule.HeaderName];

            // var requestBearerRole = "";     // TODO: Read this
            // 
            // return requestBearerRole == authorizationRule.BearerRole;
        }
    }
}
