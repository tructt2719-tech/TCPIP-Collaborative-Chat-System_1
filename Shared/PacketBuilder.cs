using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketBuilder
    {
        public static string BuildMessage(
            string sender,
            string message)
        {
            return $"MESSAGE|{sender}|{message}\n";
        }
    }
    // ── Giai đoạn 2.5 Server gắn sender vào message ──
    // Ví dụ: MSG|Alice|Hello everyone
    public static string MessageWithoutSender(string sender, string content)
    => $"{PacketTypes.MSG}|{sender}|{content}\n}";
}