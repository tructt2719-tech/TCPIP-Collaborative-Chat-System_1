using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class ClientSession
    {
        public string Username { get; set; }
        public string ConnectionId { get; private set; }
        public TcpClient TcpClient { get; set; }
        public NetworkStream Stream { get; set; }
        public bool IsLoggedIn { get; set; } = false;
        public string CurrentRoom { get; set; }
        public DateTime ConnectedAt { get; set; }
        public Socket Socket { get; set; }
        public ClientSession(TcpClient client)
        {
            if (client == null) throw new ArgumentNullException("client");
            TcpClient = client;
            Stream = client.GetStream();
            ConnectionId = "CONN-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            ConnectedAt = DateTime.Now;
        }
        public override string ToString()
        {
            return IsLoggedIn ? $"[Session: {Username} ({ConnectionId})]" : $"[Session: {ConnectionId}]";
            
        }
    }
}
