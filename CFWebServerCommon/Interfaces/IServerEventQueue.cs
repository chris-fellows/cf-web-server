using CFWebServer.Enums;
using CFWebServer.Models;

namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Interface for server events
    /// </summary>
    public interface IServerEventQueue
    {
        /// <summary>
        /// Adds event to queue
        /// </summary>
        /// <param name="serverEvent"></param>
        void Add(ServerEvent serverEvent);

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
