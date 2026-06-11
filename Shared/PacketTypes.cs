using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketTypes
    {
        public const string Login = "LOGIN";
        public const string LoginOk = "LOGIN_OK";
        public const string LoginFail = "LOGIN_FAIL";
        public const string Message = "MESSAGE";
        public const string RoomMessage = "ROOM_MESSAGE";
        public const string Disconnect = "DISCONNECT";
        public const string System = "SYSTEM";      // thông báo user join/leave
        public const string UserList = "USER_LIST";   // danh sách online
        public const string UserJoined = "USER_JOINED"; // thông báo user mới join
        public const string UserLeft = "USER_LEFT";   // thông báo user rời
        public const char Delimiter = '|';
        public const string NewLine = "\n";
    }
}
