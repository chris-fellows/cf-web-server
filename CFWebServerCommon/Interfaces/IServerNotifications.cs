using CFWebServer.Enums;
using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Interface for server notifications
    /// </summary>
    public interface IServerNotifications
    {
        /// <summary>
        /// Notifies subscriber(s) of event
        /// </summary>
        /// <param name="serverEvent"></param>
        void Notify(ServerEvent serverEvent);

        /// <summary>
        /// Subscribe to receive event notifications
        /// </summary>
        /// <param name="serverEventTypes"></param>
        /// <param name="action"></param>
        /// <returns>Subscribe Id (Passed to Unsubscribe later)</returns>
        string Subscribe(ServerEventTypes serverEventType, Action<ServerEvent> action);

        /// <summary>
        /// Unsubscribes from receiving events
        /// </summary>
        /// <param name="id">Id returned by Subscribe call</param>
        void Unsubscribe(string id);
    }
}
