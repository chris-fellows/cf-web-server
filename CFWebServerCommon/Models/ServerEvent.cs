using CFWebServer.Enums;

namespace CFWebServer.Models
{
    /// <summary>
    /// Details of server event
    /// </summary>
    public class ServerEvent
    {
        /// <summary>
        /// Event type
        /// </summary>
        public ServerEventTypes EventType { get; set; }

        /// <summary>
        /// Parameters. Values specific to event type.
        /// </summary>
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }
}
