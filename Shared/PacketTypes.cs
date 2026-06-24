using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketTypes
    {
        public const char Separator = '|';
        public const string Sep = "|";
        public const string NewLine = "\n";
        public const string Login = "LOGIN";
        public const string LoginOk = "LOGIN_OK";
        public const string LoginFail = "LOGIN_FAIL";
        public const string Register = "REGISTER";
        public const string RegisterOk = "REGISTER_OK";
        public const string RegisterFail = "REGISTER_FAIL";
        public const string Message = "MESSAGE";
        public const string RoomMessage = "ROOM_MESSAGE";
        public const string Disconnect = "DISCONNECT";
        public const string System = "SYSTEM";      // thông báo user join/leave
        public const string UserList = "USER_LIST";   // danh sách online
        public const string UserJoined = "USER_JOINED"; // thông báo user mới join
        public const string UserLeft = "USER_LEFT";   // thông báo user rời
        public const char Delimiter = '|';
        public const string NewLine = "\n";

        public const string System = "SYSTEM";
        public const string UserList = "USER_LIST";

        public const string CreateRoom = "CREATE_ROOM";
        public const string CreateRoomOk = "CREATE_ROOM_OK";
        public const string JoinRoom = "JOIN_ROOM";
        public const string JoinRoomOk = "JOIN_ROOM_OK";
        public const string LeaveRoom = "LEAVE_ROOM";
        public const string LeaveRoomOk = "LEAVE_ROOM_OK";
        public const string GetRooms = "GET_ROOMS";
        public const string RoomList = "ROOM_LIST";
        public const string RoomCreated = "ROOM_CREATED";
        public const string RoomSystem = "ROOM_SYSTEM"; // thông báo user join/leave trong room
        public const string RoomUserJoined = "ROOM_USER_JOINED"; // thông báo user mới join room
        public const string RoomUserLeft = "ROOM_USER_LEFT";   // thông báo user rời room
        public const string RelyMessage = "RELY_MESSAGE"; // tin nhắn trả lời
        public const string RoomRelyMessage = "ROOM_RELY_MESSAGE"; // tin nhắn trả lời trong room
        public const string FowardMessage = "FORWARD_MESSAGE"; // chuyển tiếp tin nhắn
        public const string RoomFowardMessage = "ROOM_FORWARD_MESSAGE"; // chuyển tiếp tin nhắn trong room
        public const string Error = "ERROR";
    }
}