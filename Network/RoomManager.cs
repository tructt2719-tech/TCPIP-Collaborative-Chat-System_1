using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TCPIP_Collaborative_Chat_System.Models;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class RoomManager
    {
        public const string DefaultRoomName = "General";

        private readonly ConcurrentDictionary<string, ChatRoomModel> _rooms =
            new ConcurrentDictionary<string, ChatRoomModel>(StringComparer.OrdinalIgnoreCase);

        public RoomManager()
        {
            _rooms[DefaultRoomName] = new ChatRoomModel(DefaultRoomName);
        }

        public enum CreateResult { Created, AlreadyExists, InvalidName }
        public enum JoinResult { Joined, RoomNotFound, NotLoggedIn }
        public enum LeaveResult { Left, NotInRoom, RoomNotFound }

        public CreateResult CreateRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName) || roomName.Contains("|"))
                return CreateResult.InvalidName;

            roomName = roomName.Trim();
            var room = new ChatRoomModel(roomName);

            return _rooms.TryAdd(roomName, room)
                ? CreateResult.Created
                : CreateResult.AlreadyExists;
        }

        public bool RoomExists(string roomName)
            => !string.IsNullOrWhiteSpace(roomName) && _rooms.ContainsKey(roomName.Trim());

        public JoinResult Join(ClientHandler client, string roomName)
        {
            if (!client.IsLoggedIn)
                return JoinResult.NotLoggedIn;

            if (string.IsNullOrWhiteSpace(roomName) || !_rooms.TryGetValue(roomName.Trim(), out var room))
                return JoinResult.RoomNotFound;

            LeaveCurrentRoom(client);

            room.AddMember(client);
            client.CurrentRoom = room.RoomName;
            return JoinResult.Joined;
        }

        public LeaveResult Leave(ClientHandler client, string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName) || !_rooms.TryGetValue(roomName.Trim(), out var room))
                return LeaveResult.RoomNotFound;

            bool removed = room.RemoveMember(client);
            if (client.CurrentRoom != null &&
                client.CurrentRoom.Equals(room.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                client.CurrentRoom = null;
            }

            return removed ? LeaveResult.Left : LeaveResult.NotInRoom;
        }

        public string LeaveCurrentRoom(ClientHandler client)
        {
            string current = client.CurrentRoom;
            if (string.IsNullOrEmpty(current))
                return null;

            if (_rooms.TryGetValue(current, out var room))
                room.RemoveMember(client);

            client.CurrentRoom = null;
            return current;
        }

        public ChatRoomModel GetRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
                return null;

            _rooms.TryGetValue(roomName.Trim(), out var room);
            return room;
        }

        public List<string> GetRoomNames()
            => _rooms.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();

        public List<ClientHandler> GetMembersOf(string roomName)
        {
            var room = GetRoom(roomName);
            return room?.GetMembers() ?? new List<ClientHandler>();
        }
    }
}