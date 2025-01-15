using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Authorization manager. E.g. API key, bearer token etc
    /// </summary>
    public interface IAuthorizationManager
    {
        /// <summary>
        /// Authorization scheme
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// Whether request valid for authorization rule
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="authorizationRules"></param>
        /// <returns></returns>
        bool IsAuthorized(RequestContext requestContext, AuthorizationRule authorizationRule);
    }
}
