using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketTypes
    {
        public const string Login = "LOGIN";

        public const string Message = "MESSAGE";

        public const string Disconnect = "DISCONNECT";
    }
}
