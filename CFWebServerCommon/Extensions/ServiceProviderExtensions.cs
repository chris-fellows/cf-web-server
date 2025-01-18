using CFWebServer.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceScope CreateSiteConfigScope(this IServiceProvider serviceProvider,string siteConfigId)
        {
            var scope = serviceProvider.CreateScope();
            var siteContext = scope.ServiceProvider.GetRequiredService<ISiteContext>();
            siteContext.SiteConfigId = siteConfigId;
            return scope;
        }
    }
}
