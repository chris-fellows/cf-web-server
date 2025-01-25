using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Models
{
    public class SiteParameter : ICloneable
    {
        public string Name { get; set; } = String.Empty;

        public string Value { get; set; } = String.Empty;

        public object Clone()
        {
            var siteParameter = new SiteParameter()
            {
                Name = Name,
                Value = Value
            };
            return siteParameter;
        }
    }
}
