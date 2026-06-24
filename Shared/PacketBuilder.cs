using System.Collections.Generic;
using System.Linq;
using TCPIP_Collaborative_Chat_System.Models;


namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketBuilder
    {
        public static string BuildLogin(string username, string password)
            => $"LOGIN|{username}|{password}\n";

        public static string BuildRegister(
            string username,
            string password,
            string email,
            string avatarBase64)
            => $"REGISTER|{username}|{password}|{email}|{avatarBase64}\n";

        public static string BuildLoginOk(string username, string avatarBase64 = "")
            => $"LOGIN_OK|{username}|{avatarBase64}\n";

        public static string BuildLoginFail(string reason)
            => $"LOGIN_FAIL|{reason}\n";

        public static string BuildRegisterOk(string username)
            => $"REGISTER_OK|{username}\n";

        public static string BuildRegisterFail(string reason)
            => $"REGISTER_FAIL|{reason}\n";

        public static string BuildSystem(string message)
            => $"SYSTEM|{message}\n";

        public static string BuildUserList(IEnumerable<UserListEntry> users)
        {
            var entries = users.Select(u => $"{u.Username}:{u.AvatarBase64}");
            return $"USER_LIST|{string.Join("|", entries)}\n";
        }

        public static string BuildMessage(string messageId, string sender, string message)
            => $"MESSAGE|{messageId}|{sender}|{message}\n";

        public static string BuildReplyMessage(string messageId, string sender, string content, string replyToId, string replyToSender, string replyToPreview)
            => $"RELY_MESSAGE|{messageId}|{sender}|{content}|{replyToId}|{replyToSender}|{replyToPreview}\n";

        public static string BuildForwardMessage(string messageId, string sender, string content, string originalSender)
            => $"FORWARD_MESSAGE|{messageId}|{sender}|{content}|{originalSender}\n";

        public static string BuildRelyMessageBroadcast(string sender, string content, string replyToId, string replyToSender)
            => $"ROOM_RELY_MESSAGE|{sender}|{content}|{replyToId}|{replyToSender}\n";

        public static string BuildEmojiReaction(string messageId, string reactor, string emoji)
            => $"EMOJI_REACTION|{messageId}|{reactor}|{emoji}";

        public static string BuildEmojiReactionBroadcast(string messageId, string reactor, string emoji)
            => $"EMOJI_REACTION_BROADCAST|{messageId}|{reactor}|{emoji}\n";

        public static string BuildCreateRoom(string roomName) => $"CREATE_ROOM|{roomName}\n";
        public static string BuildCreateRoomOk(string roomName) => $"CREATE_ROOM_OK|{roomName}\n";
        public static string BuildRoomCreated(string roomName) => $"ROOM_CREATED|{roomName}\n";

        public static string BuildJoinRoom(string roomName) => $"JOIN_ROOM|{roomName}\n";
        public static string BuildJoinRoomOk(string roomName) => $"JOIN_ROOM_OK|{roomName}\n";

        public static string BuildLeaveRoom(string roomName) => $"LEAVE_ROOM|{roomName}\n";
        public static string BuildLeaveRoomOk(string roomName) => $"LEAVE_ROOM_OK|{roomName}\n";

        public static string BuildGetRooms() => "GET_ROOMS\n";

        public static string BuildRoomList(IEnumerable<string> roomNames)
            => $"ROOM_LIST|{string.Join("|", roomNames)}\n";

        public static string BuildRoomSystem(string roomName, string message)
            => $"ROOM_SYSTEM|{roomName}|{message}\n";

        public static string BuildRoomUserJoined(string roomName, string username)
            => $"ROOM_USER_JOINED|{roomName}|{username}\n";

        public static string BuildRoomUserLeft(string roomName, string username)
            => $"ROOM_USER_LEFT|{roomName}|{username}\n";

        public static string BuildRoomUserList(string roomName, IEnumerable<string> usernames)
            => $"ROOM_USER_LIST|{roomName}|{string.Join("|", usernames)}\n";

        public static string BuildError(string code, string detail) => $"ERROR|{code}|{detail}\n";

        public static string BuildRoomMessage(string roomName, string messageId, string sender, string content)
            => $"{TCPIP_Collaborative_Chat_System.Shared.PacketTypes.RoomMessage}|{roomName}|{messageId}|{sender}|{content}\n";

        public static string BuildRoomReplyMessage(
            string roomName,
            string messageId,
            string sender,
            string content,
            string replyToId,
            string replyToSender,
            string replyToPreview)
            => $"ROOM_RELY_MESSAGE|{roomName}|{messageId}|{sender}|{content}|{replyToId}|{replyToSender}|{replyToPreview}\n";

        public static string BuildRoomForwardMessage(
            string roomName,
            string messageId,
            string sender,
            string content,
            string originalSender)
            => $"ROOM_FORWARD_MESSAGE|{roomName}|{messageId}|{sender}|{content}|{originalSender}\n";

        public static string BuildRoomEmojiReaction(string roomName, string messageId, string reactor, string emoji)
            => $"EMOJI_REACTION|{roomName}|{messageId}|{reactor}|{emoji}\n";

        public static string BuildRoomEmojiReactionBroadcast(string roomName, string messageId, string reactor, string emoji)
            => $"EMOJI_REACTION_BROADCAST|{roomName}|{messageId}|{reactor}|{emoji}\n";

    }
    public struct UserListEntry
    {
        public string Username { get; }
        public string AvatarBase64 { get; }

        public UserListEntry(string username, string avatarBase64)
        {
            Username = username ?? string.Empty;
            AvatarBase64 = avatarBase64 ?? string.Empty;
        }
    }
}