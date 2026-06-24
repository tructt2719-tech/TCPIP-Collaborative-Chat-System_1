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
<<<<<<< HEAD
        public const string System = "SYSTEM";      // thông báo user join/leave
        public const string UserList = "USER_LIST";   // danh sách online
        public const string UserJoined = "USER_JOINED"; // thông báo user mới join
        public const string UserLeft = "USER_LEFT";   // thông báo user rời
        public const char Delimiter = '|';
        public const string NewLine = "\n";

        public const string System = "SYSTEM";
        public const string UserList = "USER_LIST";
=======

        public const string System = "SYSTEM";          // thông báo user join/leave
        public const string UserList = "USER_LIST";      // danh sách online (kèm avatar)
        public const string UserJoined = "USER_JOINED";  // thông báo user mới join
        public const string UserLeft = "USER_LEFT";      // thông báo user rời

        public const char Delimiter = '|';
>>>>>>> 5b3c3a5ca6a355b2a60fd59bcd4a64abad91618c

        public const string CreateRoom = "CREATE_ROOM";
        public const string CreateRoomOk = "CREATE_ROOM_OK";
        public const string JoinRoom = "JOIN_ROOM";
        public const string JoinRoomOk = "JOIN_ROOM_OK";
        public const string LeaveRoom = "LEAVE_ROOM";
        public const string LeaveRoomOk = "LEAVE_ROOM_OK";
        public const string GetRooms = "GET_ROOMS";
        public const string RoomList = "ROOM_LIST";
        public const string RoomCreated = "ROOM_CREATED";
        public const string RoomSystem = "ROOM_SYSTEM";
        public const string RoomUserJoined = "ROOM_USER_JOINED";
        public const string RoomUserLeft = "ROOM_USER_LEFT";
        public const string RoomUserList = "ROOM_USER_LIST";     // danh sách thành viên của 1 room cụ thể
        public const string ErrorRoomExists = "ROOM_EXISTS";
        public const string ErrorRoomNotFound = "ROOM_NOT_FOUND";
        public const string ErrorNotInRoom = "NOT_IN_ROOM";

        // Reply / Forward / Emoji — message-id based realtime features
        public const string ReplyMessage = "RELY_MESSAGE";              // tin nhắn trả lời
        public const string RoomReplyMessage = "ROOM_RELY_MESSAGE";     // (room) chưa dùng trong bản hiện tại
        public const string ForwardMessage = "FORWARD_MESSAGE";         // chuyển tiếp tin nhắn
        public const string RoomForwardMessage = "ROOM_FORWARD_MESSAGE";// (room) chưa dùng trong bản hiện tại

        public const string EmojiReaction = "EMOJI_REACTION";                     // client -> server
        public const string EmojiReactionBroadcast = "EMOJI_REACTION_BROADCAST";  // server -> clients

        public const string Error = "ERROR";
<<<<<<< HEAD
=======

        // Giữ alias cũ để không phá code khác lỡ còn tham chiếu PacketTypes.RelyMessage / FowardMessage
        public const string RelyMessage = ReplyMessage;
        public const string FowardMessage = ForwardMessage;
>>>>>>> 5b3c3a5ca6a355b2a60fd59bcd4a64abad91618c
    }
}
