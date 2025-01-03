using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Web server component
    /// </summary>
    internal interface IWebServerComponent
    {
        /// <summary>
        /// Start component
        /// </summary>
        void Start();

        /// <summary>
        /// Stop component
        /// </summary>
        void Stop();
    }
}
