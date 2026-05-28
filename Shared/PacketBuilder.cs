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
        public static string BuildLoginOk(string username)
            => $"LOGIN_OK|{username}\n";

        public static string BuildLoginFail(string reason)
            => $"LOGIN_FAIL|{reason}\n";

        public static string BuildSystem(string message)
            => $"SYSTEM|{message}\n";

        public static string BuildUserList(IEnumerable<string> usernames)
            => $"USER_LIST|{string.Join("|", usernames)}\n";
    }
}