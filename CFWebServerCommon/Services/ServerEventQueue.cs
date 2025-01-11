using CFWebServer.Enums;
using CFWebServer.Interfaces;
using CFWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFWebServer.Services
{
    public class ServerEventQueue : IServerEventQueue
    {
        private class Subscription
        {
            public string Id { get; internal set; }

            public ServerEventTypes EventType { get; internal set; }

            public Action<ServerEvent> Action { get; internal set; }

            public Subscription(ServerEventTypes eventType, Action<ServerEvent> action)
            {
                Id = Guid.NewGuid().ToString();
                EventType = eventType;
                Action = action;
            }
        }

        private List<Subscription> _subscriptions = new List<Subscription>();

        public void Add(ServerEvent serverEvent)
        {
            var subscriptions = _subscriptions.Where(s => s.EventType == serverEvent.EventType);
            foreach(var subscription in subscriptions)
            {
                subscription.Action(serverEvent);
            }               
        }

        public string Subscribe(ServerEventTypes serverEventType, Action<ServerEvent> action)
        {
            var subscription = new Subscription(serverEventType, action);
            _subscriptions.Add(subscription);
            return subscription.Id;              
        }

        public void Unsubscribe(string id)
        {
            _subscriptions.RemoveAll(s => s.Id == id);
        }
    }
}
