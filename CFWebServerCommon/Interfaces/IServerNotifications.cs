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
        /// Subscribes to receive events
        /// </summary>
        /// <param name="serverEventTypes"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        string Subscribe(ServerEventTypes serverEventType, Action<ServerEvent> action);

        /// <summary>
        /// Unsubscribes from receiving events
        /// </summary>
        /// <param name="id"></param>
        void Unsubscribe(string id);
    }
}
