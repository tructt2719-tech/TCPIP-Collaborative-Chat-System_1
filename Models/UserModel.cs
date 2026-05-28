using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    internal class UserModel
    {
        public string Username { get; set; }
        public Socket Socket { get; set; }
        public string CurrentRoom { get; set; }
        public bool IsLoggedIn { get; set; } = false;
    }
}
