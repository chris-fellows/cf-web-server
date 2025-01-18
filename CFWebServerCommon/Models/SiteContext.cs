using CFWebServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Models
{
    public class SiteContext : ISiteContext
    {
        public string SiteConfigId { get; set; } = String.Empty;
    }
}
