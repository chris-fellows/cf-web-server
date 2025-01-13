using CFWebServer.Models;
using CFWebServerCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Interfaces
{
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
