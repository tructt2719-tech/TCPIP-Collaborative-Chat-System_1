using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    internal class PacketBuilder
    {
    }
}
public static class PacketBuilder
{
    public static string BuildMessage(
        string sender,
        string message)
    {
        return $"MESSAGE|{sender}|{message}\n";
    }
}