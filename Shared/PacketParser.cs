using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    internal class PacketParser
    {
    }
}
public static class PacketParser
{
    public static string[] Parse(string packet)
    {
        return packet.Split('|');
    }
}
