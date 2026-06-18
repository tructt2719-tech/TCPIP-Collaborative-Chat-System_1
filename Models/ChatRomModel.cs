using System;
using System.Collection.Generic;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class ChatRoomModel
    {
        public string RoomName { get; set; }
        public DateTime CreateAt { get; set; }

        private readonly List<ClientSession> _members = new List<ClientSession>();
        private readonly object _lock = new object();

        public ChatRoomModel(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
            RoomName = name;
            CreateAt = DateTime.Now;
        }
        public bool AddMember(ClientSession s)
        {
            lock (_lock) { if (_members.Contains(s)) return false; _members.Add(s); return true; }
        }
        public bool RemoveMember(ClientSession s)
        {
            lock (_lock) { return _members.Remove(s); }
        }

        public bool HasMember(ClientSession s)
        {
            lock (_lock) { return _members.Contains(s); }
        }

        public List<ClientSession>

    }
}