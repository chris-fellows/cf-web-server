using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Interface to web server
    /// </summary>
    public interface IWebServer
    {
        void Start();

        void Stop();
    }
}
