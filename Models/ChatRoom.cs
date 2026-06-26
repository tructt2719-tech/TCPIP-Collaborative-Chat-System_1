using System.Collections.Generic;
using TCPIP_Collaborative_Chat_System.Network;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class ChatRoom
    {
        public string RoomName { get; set; }
        public string Owner { get; set; }
        public int MaxUsers { get; set; }
        public bool IsPrivate { get; set; }
        public string Password { get; set; }
        public List<ClientHandler> Members { get; set; }
            = new List<ClientHandler>();

    }
}