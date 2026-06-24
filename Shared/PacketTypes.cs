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
        public const string CreateRoom = "CREATE_ROOM";
        public const string CreateRoomOk = "CREATE_ROOM_OK";
        public const string RoomExists = "ROOM_EXISTS";
        public const string JoinRoom = "JOIN_ROOM";
        public const string JoinRoomOk = "JOIN_ROOM_OK";

        public const string LeaveRoom = "LEAVE_ROOM";
        public const string LeaveRoomOk = "LEAVE_ROOM_OK";

        public const string RoomList = "ROOM_LIST";

        public const string RoomMsg = "ROOM_MSG";

        public const string RoomCreated = "ROOM_CREATED";

        public const string RoomUserJoined = "ROOM_USER_JOINED";
        public const string RoomUserLeft = "ROOM_USER_LEFT";

        public const string GetRooms = "GET_ROOMS";
        public const string RoomUsers = "ROOM_USERS";
    }

}
