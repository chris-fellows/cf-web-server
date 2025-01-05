using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Models
{
    /// <summary>
    /// Web server statistics
    /// </summary>
    public class ServerStatistics
    {
        /// <summary>
        /// Server start time
        /// </summary>
        public DateTimeOffset StartedTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Number of requests
        /// </summary>
        public int CountRequestsReceived { get; set; }

        /// <summary>
        /// Time of last request
        /// </summary>
        public DateTimeOffset? LastRequestReceivedTime { get; set; }
    }
}
