using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPIP_Collaborative_Chat_System.Models;

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

        public static string BuildCreateRoomOk(string roomName)
            => $"CREATE_ROOM_OK|{roomName}\n";

        public static string BuildRoomExists(string roomName)
            => $"ROOM_EXISTS|{roomName}\n";
        public static string BuildJoinRoomOk(string roomName)
            => $"JOIN_ROOM_OK|{roomName}\n";
        public static string BuildLeaveRoomOk(string roomName)
            => $"LEAVE_ROOM_OK|{roomName}\n";
        public static string BuildRoomUsers(string roomName, IEnumerable<string> users)
        {
            return $"ROOM_USERS|{roomName}|{string.Join("|", users)}\n";
        }
        public static string BuildRoomUserJoined(string roomName, string username)
        {
            return $"ROOM_USER_JOINED|{roomName}|{username}\n";
        }
        public static string BuildRoomUserLeft(string roomName, string username)
        {
            return $"ROOM_USER_LEFT|{roomName}|{username}\n";
        }
        public static string BuildRoomList(IEnumerable<ChatRoom> rooms)
        {
            StringBuilder sb = new StringBuilder("ROOM_LIST");
            foreach (var room in rooms)
            {
                sb.Append("|");
                sb.Append(room.RoomName);
                sb.Append(",");
                sb.Append(room.IsPrivate
                    ? "PRIVATE"
                    : "PUBLIC");
            }
            sb.Append("\n");
            return sb.ToString();
        }
        public static string BuildRegisterOk(string username)
        {
            return $"REGISTER_OK|{username}\n";
        }

        public static string BuildRegisterFail(string reason)
        {
            return $"REGISTER_FAIL|{reason}\n";
        }
        public static string BuildRoomHistory(string roomName, string sender, string content, string time, Guid messageId, bool isReply, Guid? replyToMessageId, bool isForward, Guid? forwardMessageId)
        {
            string replyIdStr = replyToMessageId.HasValue ? replyToMessageId.Value.ToString() : "";
            string forwardIdStr = forwardMessageId.HasValue ? forwardMessageId.Value.ToString() : "";
            return $"ROOM_HISTORY|{roomName}|{sender}|{content}|{time}|{messageId}|{(isReply ? 1 : 0)}|{replyIdStr}|{(isForward ? 1 : 0)}|{forwardIdStr}\n";
        }

        public static string BuildDeleteRoomOk(string roomName)
            => $"DELETE_ROOM_OK|{roomName}\n";

        public static string BuildDeleteRoomFail(string reason)
            => $"DELETE_ROOM_FAIL|{reason}\n";

        public static string BuildRoomDeleted(string roomName, string deletedBy)
            => $"ROOM_DELETED|{roomName}|{deletedBy}\n";

        public static string BuildFileInfo(
            string roomName,
            string sender,
            string fileName,
            long fileSize)
        {
            return $"FILE_INFO|{roomName}|{sender}|{fileName}|{fileSize}\n";
        }
        public static string BuildFileDownload(string fileName)
        {
            return $"FILE_DOWNLOAD|{fileName}\n";
        }
        public static string BuildFileData(string fileName, string base64)
        {
            return $"FILE_DATA|{fileName}|{base64}\n";
        }

        // Reply & Forward
        public static string BuildRoomMsg(Guid messageId, string roomName, string sender, string content)
            => $"ROOM_MSG|{messageId}|{roomName}|{sender}|{content}\n";

        public static string BuildReplyMsg(Guid newMessageId, Guid replyToMessageId, string roomName, string sender, string content)
            => $"REPLY_MSG|{newMessageId}|{replyToMessageId}|{roomName}|{sender}|{content}\n";

        public static string BuildPrivateReply(Guid newMessageId, Guid replyToMessageId, string targetUser, string sender, string content)
            => $"PRIVATE_REPLY|{newMessageId}|{replyToMessageId}|{targetUser}|{sender}|{content}\n";

        public static string BuildForwardMsg(Guid newMessageId, Guid originalMessageId, string targetRoom, string sender)
            => $"FORWARD_MSG|{newMessageId}|{originalMessageId}|{targetRoom}|{sender}\n";

        public static string BuildForwardPrivate(Guid newMessageId, Guid originalMessageId, string targetUser, string sender)
            => $"FORWARD_PRIVATE|{newMessageId}|{originalMessageId}|{targetUser}|{sender}\n";

        public static string BuildDeleteMsg(Guid messageId, string roomName)
            => $"DELETE_MSG|{messageId}|{roomName}\n";
    }
}