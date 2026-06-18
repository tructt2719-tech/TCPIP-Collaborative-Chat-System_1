using System.Collections.Generic;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketBuilder
    {
<<<<<<< Updated upstream
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

=======
        private static string Build(params string[] parts)
        {
            return string.Join(PacketTypes.Sep, parts) + "\n";
        }

        public static string BuildMessage(
            string sender,
            string message)
        {
            return $"MESSAGE|{sender}|{message}\n";
        }
>>>>>>> Stashed changes
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
        public static string BuildReplyMessage(string sender, string content, string replyToId, string replyToSender)
            => $"RELY_MESSAGE|{sender}|{content}|{replyToId}|{replyToSender}\n";

        public static string BuildForwardMessage(string sender, string content, string originalSender)
            => $"FORWARD_MESSAGE|{sender}|{content}|{originalSender}\n";

        public static string BuildRelyMessageBroadcast(string sender, string content, string replyToId, string replyToSender)
            => $"ROOM_RELY_MESSAGE|{sender}|{content}|{replyToId}|{replyToSender}\n";

        public static string BuildEmojiReaction(string sender, string emoji, string messageId)
            => $"EMOJI_REACTION|{sender}|{emoji}|{messageId}\n";
    }
}