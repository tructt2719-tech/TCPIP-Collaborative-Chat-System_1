using System.Collections.Generic;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketBuilder
    {
        public static string BuildMessage(string sender, string message)
            => $"MESSAGE|{sender}|{message}\n";

        public static string BuildLogin(string username, string password)
            => $"LOGIN|{username}|{password}\n";

        public static string BuildRegister(
            string username,
            string password,
            string email,
            string avatarBase64)
            => $"REGISTER|{username}|{password}|{email}|{avatarBase64}\n";

        public static string BuildLoginOk(string username)
            => $"LOGIN_OK|{username}\n";

        public static string BuildLoginFail(string reason)
            => $"LOGIN_FAIL|{reason}\n";

        public static string BuildRegisterOk(string username)
            => $"REGISTER_OK|{username}\n";

        public static string BuildRegisterFail(string reason)
            => $"REGISTER_FAIL|{reason}\n";

        public static string BuildSystem(string message)
            => $"SYSTEM|{message}\n";

        public static string BuildUserList(IEnumerable<string> usernames)
            => $"USER_LIST|{string.Join("|", usernames)}\n";
    }
}