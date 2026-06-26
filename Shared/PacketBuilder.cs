using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static string BuildRoomHistory(string roomName, string sender, string content, string time)
        {
            return $"ROOM_HISTORY|{roomName}|{sender}|{content}|{time}\n";
        }

    }
}