using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketParser
    {
        public static string[] Parse(string packet)
        {
            if (string.IsNullOrEmpty(packet)) return new string[0];
            return packet.Split('|');
        }
    }
}
