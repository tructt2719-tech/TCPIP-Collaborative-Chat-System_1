using System;
using System.Collections.Generic;
using System.Linq;
using TCPIP_Collaborative_Chat_System.Network;

namespace TCPIP_Collaborative_Chat_System.Models
{
    /// <summary>
    /// Đại diện 1 phòng chat (Giai đoạn 3.1). Lưu danh sách ClientHandler (kết nối socket
    /// thật của Server, namespace Network) đang là thành viên của Room này.
    ///
    /// Ghi chú: bản gốc thiết kế ChatRoomModel quanh ClientSession (TcpClient-based), nhưng
    /// TcpChatServer của project dùng Socket thô + ClientHandler, nên Room được viết lại để
    /// khớp với kiến trúc Server đang chạy thật, tránh có 2 khái niệm "session" song song.
    /// </summary>
    public class ChatRoomModel
    {
        public string RoomName { get; }
        public DateTime CreatedAt { get; }

        private readonly List<ClientHandler> _members = new List<ClientHandler>();
        private readonly object _lock = new object();

        public ChatRoomModel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Room name không được rỗng", nameof(name));

            RoomName = name;
            CreatedAt = DateTime.Now;
        }

        public bool AddMember(ClientHandler client)
        {
            lock (_lock)
            {
                if (_members.Contains(client)) return false;
                _members.Add(client);
                return true;
            }
        }

        public bool RemoveMember(ClientHandler client)
        {
            lock (_lock) { return _members.Remove(client); }
        }

        public bool HasMember(ClientHandler client)
        {
            lock (_lock) { return _members.Contains(client); }
        }

        public List<ClientHandler> GetMembers()
        {
            lock (_lock) { return new List<ClientHandler>(_members); }
        }

        public List<string> GetMemberUsernames()
        {
            lock (_lock) { return _members.Where(m => m.IsLoggedIn).Select(m => m.Username).ToList(); }
        }

        public int MemberCount { get { lock (_lock) { return _members.Count; } } }
    }
}
