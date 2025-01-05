using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Models
{
    public class RequestContext
    {
        public HttpListenerRequest Request { get; internal set; }
        
        public HttpListenerResponse Response { get; internal set; }

        public RequestContext(HttpListenerRequest request, HttpListenerResponse response)
        {
            Request = request; 
            Response = response;
        }
    }
}
