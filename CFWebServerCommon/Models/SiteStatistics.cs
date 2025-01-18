namespace CFWebServer.Models
{
    /// <summary>
    /// Site statistics
    /// </summary>
    public class SiteStatistics
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
