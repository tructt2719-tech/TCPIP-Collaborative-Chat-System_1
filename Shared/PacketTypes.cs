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
        public const string Disconnect = "DISCONNECT";
        public const string System = "SYSTEM";      // thông báo user join/leave
        public const string UserList = "USER_LIST";   // danh sách online
    }
}
