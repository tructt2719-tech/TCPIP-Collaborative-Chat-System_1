using System.Collections.Concurrent;
using System.Collections.Generic;
using TCPIP_Collaborative_Chat_System.Models;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class MessageHistoryStore
    {
        private const int MaxKeep = 500;

        private readonly ConcurrentDictionary<string, MessageModel> _byId =
            new ConcurrentDictionary<string, MessageModel>();

        private readonly Queue<string> _order = new Queue<string>();
        private readonly object _lock = new object();

        public void Add(MessageModel message)
        {
            if (message == null || string.IsNullOrEmpty(message.MessageId))
                return;

            _byId[message.MessageId] = message;

            lock (_lock)
            {
                _order.Enqueue(message.MessageId);
                while (_order.Count > MaxKeep)
                {
                    string oldest = _order.Dequeue();
                    _byId.TryRemove(oldest, out _);
                }
            }
        }

        public bool TryGet(string messageId, out MessageModel message)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                message = null;
                return false;
            }

            return _byId.TryGetValue(messageId, out message);
        }
    }
}