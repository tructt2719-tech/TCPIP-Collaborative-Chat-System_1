using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Shared;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Database;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class TcpChatServer
    {
        public TcpChatServer()
        {
           
        }

        // Events to notify UI - no direct UI dependency
        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<string> OnUserListChanged;

        private Socket _serverSocket;
        private readonly List<ClientHandler> _clients = new List<ClientHandler>();
        private readonly List<ChatRoom> _rooms = new List<ChatRoom>();
        private readonly RoomRepository _roomRepo = new RoomRepository();
       
        private const int MAX_ROOMS = 20;
        public void Start(int port)
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _serverSocket.Listen(10);
            if (!_roomRepo.RoomExists("Study"))
            {
                _roomRepo.AddRoom(
                    new ChatRoom
                    {
                        RoomName = "Study",
                        Owner = "SYSTEM",
                        MaxUsers = 10,
                        IsPrivate = false
                    });

                _roomRepo.AddRoom(
                    new ChatRoom
                    {
                        RoomName = "Music",
                        Owner = "SYSTEM",
                        MaxUsers = 10,
                        IsPrivate = false
                    });

                _roomRepo.AddRoom(
                    new ChatRoom
                    {
                        RoomName = "Team",
                        Owner = "SYSTEM",
                        MaxUsers = 10,
                        IsPrivate = false
                    });

                _roomRepo.AddRoom(
                    new ChatRoom
                    {
                        RoomName = "Gaming",
                        Owner = "SYSTEM",
                        MaxUsers = 10,
                        IsPrivate = false
                    });

                _roomRepo.AddRoom(
                    new ChatRoom
                    {
                        RoomName = "Work",
                        Owner = "SYSTEM",
                        MaxUsers = 10,
                        IsPrivate = false
                    });
            }
            _rooms.Clear();
            _rooms.AddRange(_roomRepo.GetAllRooms());
            _serverSocket.BeginAccept(HandleConnection, null);
            OnStatusChanged?.Invoke("Đang chờ kết nối...");
        }

        public void Stop()
        {
            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                    client.Close();
                _clients.Clear();
            }
            try { _serverSocket?.Close(); } catch { }
        }

        public void Broadcast(string packet)
        {
            if (!packet.EndsWith("\n")) packet += "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            List<ClientHandler> disconnected = new List<ClientHandler>();

            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    try
                    {
                        if (client.Socket.Connected)
                            client.Send(buffer);
                    }
                    catch
                    {
                        disconnected.Add(client);
                    }
                }
            }

            foreach (var client in disconnected)
                RemoveClient(client);
        }

        private void HandleConnection(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = _serverSocket.EndAccept(ar);
                var handler = new ClientHandler(clientSocket);

                lock (_clients)
                    _clients.Add(handler);

                OnClientConnected?.Invoke(clientSocket.RemoteEndPoint.ToString());
                OnStatusChanged?.Invoke($"{_clients.Count} client(s) đang kết nối");

                _serverSocket.BeginAccept(HandleConnection, null);

                clientSocket.BeginReceive(handler.Buffer, 0, handler.Buffer.Length,
                    SocketFlags.None, HandleDataReceived, handler);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Lỗi Accept: " + ex.Message);
            }
        }

        private void HandleDataReceived(IAsyncResult ar)
        {
            var handler = (ClientHandler)ar.AsyncState;
            try
            {
                int size = handler.Socket.EndReceive(ar);
                if (size == 0)
                {
                    RemoveClient(handler);
                    return;
                }

                string chunk = Encoding.UTF8.GetString(handler.Buffer, 0, size);
                handler.ReceiveBuffer.Append(chunk);

                ProcessReceiveBuffer(handler, line => HandleLine(handler, line));

                if (handler.Socket.Connected)
                {
                    handler.Socket.BeginReceive(handler.Buffer, 0, handler.Buffer.Length,
                        SocketFlags.None, HandleDataReceived, handler);
                }
            }
            catch
            {
                RemoveClient(handler);
            }
        }

        private void HandleLine(ClientHandler handler, string line)
        {
            string[] parts = PacketParser.Parse(line);
            if (parts.Length == 0) return;

                    string command = parts[0];
                    if (command == PacketTypes.CreateRoom && parts.Length >= 4)
                    {
                        string roomName = parts[1];

                        int maxUsers =
                            int.Parse(parts[2]);

                        bool isPrivate =
                            parts[3] == "PRIVATE";

                        string password = "";

                        if (isPrivate &&
                            parts.Length >= 5)
                        {
                            password = parts[4];
                        }

                        HandleCreateRoom(
                            handler,
                            roomName,
                            maxUsers,
                            isPrivate,
                            password);

                        return;
                    }
                    if (command == PacketTypes.JoinRoom && parts.Length >= 2)
                    {
                        string roomName = parts[1];

                        string password = "";

                        if (parts.Length > 2)
                        {
                            password = parts[2];
                        }

                        HandleJoinRoom(
                            handler,
                            roomName,
                            password);

                        return;
                    }
                    if (command == PacketTypes.LeaveRoom && parts.Length >= 2)
                    {
                        HandleLeaveRoom(
                            handler,
                            parts[1]);

                        return;
                    }
                    if (command == PacketTypes.RoomMsg && parts.Length >= 3)
                    {
                        HandleRoomMessage(
                            handler,
                            parts[1],
                            parts[2]);

                        return;
                    }
                    if (command == PacketTypes.GetRooms)
                    {
                        HandleGetRooms(handler);
                        return;
                    }

                    // REGISTER|username|password
                    if (command == PacketTypes.Register && parts.Length >= 3)
                    {
                        string username = parts[1].Trim();
                        string password = parts[2];

                        if (string.IsNullOrWhiteSpace(username))
                        {
                            SendTo(handler,
                                PacketBuilder.BuildRegisterFail("Username không được rỗng"));
                            return;
                        }

                        if (UserRepository.UserExists(username))
                        {
                            SendTo(handler,
                                PacketBuilder.BuildRegisterFail("Username đã tồn tại"));
                            return;
                        }

                        UserRepository.AddUser(username, password);

                        SendTo(handler,
                            PacketBuilder.BuildRegisterOk(username));

                        OnStatusChanged?.Invoke($"Đăng ký thành công: {username}");

                        return;
                    }

                    // LOGIN|Alice
                    if (command == PacketTypes.Login && parts.Length >= 3)
                    {
                        string username = parts[1].Trim();
                        string passwordHash = PasswordHasher.Hash(parts[2]);

            handler.Status = "Online";

            string msgId = parts[1];
            string originalMessageId = parts[2];

            string content;
            string originalSender;

            if (_history.TryGet(originalMessageId, out MessageModel original))
            {
                content = original.Content;
                originalSender = original.Sender;
            }
            else if (parts.Length >= 5)
            {
                // fallback: client tự gửi kèm content + originalSender nếu server không còn lưu message gốc
                originalSender = parts[3];
                content = parts[4];
            }
            else
            {
                SendTo(handler, $"ERROR|FORWARD_SOURCE_NOT_FOUND|{originalMessageId}\n");
                return;
            }

                        if (!UserRepository.ValidateLogin(username, passwordHash))
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Sai tài khoản hoặc mật khẩu"));
                            return;
                        }

                        // Đăng ký username thành công
                        handler.Username = username;
                        handler.CurrentRoom = null;
                        SendTo(handler, PacketBuilder.BuildLoginOk(username));
                        HandleGetRooms(handler);

        private void BroadcastRoomUserList(string roomName)
        {
            var usernames = _rooms.GetMembersOf(roomName)
                .Where(m => m.IsLoggedIn)
                .Select(m => m.Username);

            BroadcastToRoom(roomName, PacketBuilder.BuildRoomUserList(roomName, usernames));
        }

        // ROOM_MESSAGE|roomName|content   hoặc   ROOM_MESSAGE|roomName|msgId|content
        private void HandleRoomMessage(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 3) return;

            string roomName = parts[1].Trim();
            if (!RequireInRoom(handler, roomName)) return;

            string clientMsgId = null;
            string content;

            if (parts.Length >= 4)
            {
                clientMsgId = parts[2];
                content = parts[3];
            }
            else
            {
                content = parts[2];
            }

            var model = new MessageModel(handler.Username, content) { Room = roomName };
            if (!string.IsNullOrWhiteSpace(clientMsgId))
                model.MessageId = clientMsgId;

            _history.Add(model);

            string packet = PacketBuilder.BuildRoomMessage(roomName, model.MessageId, handler.Username, content);
            OnMessageReceived?.Invoke($"[{roomName}] {handler.Username}: {content}");
            BroadcastToRoom(roomName, packet);
        }

        // ROOM_RELY_MESSAGE|roomName|msgId|content|replyToId
        private void HandleRoomReplyMessage(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 5) return;

            string roomName = parts[1].Trim();
            if (!RequireInRoom(handler, roomName)) return;

            string msgId = parts[2];
            string content = parts[3];
            string replyToId = parts[4];

            string replyToSender = string.Empty;
            string replyToPreview = string.Empty;

            if (_history.TryGet(replyToId, out MessageModel original))
            {
                replyToSender = original.Sender;
                replyToPreview = MessageModel.MakePreview(original.Content);
            }

            var model = new MessageModel(handler.Username, content)
            {
                MessageId = string.IsNullOrWhiteSpace(msgId) ? Guid.NewGuid().ToString("N") : msgId,
                Room = roomName,
                IsReply = true,
                ReplyToId = replyToId,
                ReplyToSender = replyToSender,
                ReplyToPreview = replyToPreview
            };
            _history.Add(model);

            string packet = PacketBuilder.BuildRoomReplyMessage(
                roomName, model.MessageId, handler.Username, content, replyToId, replyToSender, replyToPreview);

            OnMessageReceived?.Invoke($"[{roomName}] {handler.Username} (trả lời {replyToSender}): {content}");
            BroadcastToRoom(roomName, packet);
        }

        // ROOM_FORWARD_MESSAGE|roomName|msgId|originalMessageId
        private void HandleRoomForwardMessage(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 4) return;

            string roomName = parts[1].Trim();
            if (!RequireInRoom(handler, roomName)) return;

            string msgId = parts[2];
            string originalMessageId = parts[3];

            string content;
            string originalSender;

            if (_history.TryGet(originalMessageId, out MessageModel original))
            {
                content = original.Content;
                originalSender = original.Sender;
            }
            else
            {
                SendTo(handler, PacketBuilder.BuildError("FORWARD_SOURCE_NOT_FOUND", originalMessageId));
                return;
            }

            var model = new MessageModel(handler.Username, content)
            {
                MessageId = string.IsNullOrWhiteSpace(msgId) ? Guid.NewGuid().ToString("N") : msgId,
                Room = roomName,
                IsForwarded = true,
                OriginalSender = originalSender
            };
            _history.Add(model);

            string packet = PacketBuilder.BuildRoomForwardMessage(
                roomName, model.MessageId, handler.Username, content, originalSender);

            OnMessageReceived?.Invoke($"[{roomName}] {handler.Username} (chuyển tiếp từ {originalSender}): {content}");
            BroadcastToRoom(roomName, packet);
        }

        private bool RequireInRoom(ClientHandler handler, string roomName)
        {
            if (!_rooms.RoomExists(roomName))
            {
                SendTo(handler, PacketBuilder.BuildError(PacketTypes.ErrorRoomNotFound, roomName));
                return false;
            }

            if (string.IsNullOrEmpty(handler.CurrentRoom) ||
                !handler.CurrentRoom.Equals(roomName, StringComparison.OrdinalIgnoreCase))
            {
                SendTo(handler, PacketBuilder.BuildError(PacketTypes.ErrorNotInRoom, roomName));
                return false;
            }

            return true;
        }

        private bool RequireLoggedIn(ClientHandler handler)
        {
            if (handler.IsLoggedIn) return true;
            SendTo(handler, PacketBuilder.BuildLoginFail("Bạn chưa đăng nhập"));
            return false;
        }

        private void HandleRegister(ClientHandler handler, string[] parts)
        {
            if (parts.Length < 5)
            {
                SendTo(handler, PacketBuilder.BuildRegisterFail("Packet REGISTER không hợp lệ"));
                return;
            }

            string username = parts[1].Trim();
            string password = parts[2];
            string email = parts[3].Trim();
            string avatarBase64 = parts[4];

            if (SharedUserStore.TryRegister(username, password, email, avatarBase64, out string error))
            {
                SendTo(handler, PacketBuilder.BuildRegisterOk(username));
                OnStatusChanged?.Invoke($"Đăng ký thành công: {username}");
            }
            else
            {
                SendTo(handler, PacketBuilder.BuildRegisterFail(error));
            }
        }

        private void HandleLogin(ClientHandler handler, string[] parts)
        {
            if (parts.Length < 3)
            {
                SendTo(handler, PacketBuilder.BuildLoginFail("Packet LOGIN không hợp lệ"));
                return;
            }

            string username = parts[1].Trim();
            string password = parts[2];

            if (!UserStore.ValidateUsername(username, out string usernameError))
            {
                SendTo(handler, PacketBuilder.BuildLoginFail(usernameError));
                return;
            }

            if (!SharedUserStore.TryAuthenticate(username, password, out RegisteredUser user, out string authError))
            {
                SendTo(handler, PacketBuilder.BuildLoginFail(authError));
                return;
            }

            bool isDuplicate;
            lock (_clients)
            {
                isDuplicate = _clients.Any(c => c.IsLoggedIn &&
                    c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }

            if (isDuplicate)
            {
                SendTo(handler, PacketBuilder.BuildLoginFail("Username đã online"));
                return;
            }

            handler.Username = username;
            handler.AvatarBase64 = user.AvatarBase64 ?? string.Empty;
            handler.Status = "LoggedIn";
            OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> LoggedIn");
            SendTo(handler, PacketBuilder.BuildLoginOk(username, handler.AvatarBase64));
            handler.Status = "Online";
            OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> Online");

            string joinMsg = PacketBuilder.BuildSystem($"{username} đã tham gia");
            BroadcastToLoggedIn(joinMsg);

            BroadcastUserList();

            // Giai đoạn 3: tự động đưa user vào room mặc định "General" ngay khi login,
            // để có thể chat ngay mà không cần JOIN_ROOM thủ công trước.
            var joinResult = _rooms.Join(handler, RoomManager.DefaultRoomName);
            if (joinResult == RoomManager.JoinResult.Joined)
            {
                SendTo(handler, PacketBuilder.BuildJoinRoomOk(RoomManager.DefaultRoomName));
                BroadcastToRoom(RoomManager.DefaultRoomName,
                    PacketBuilder.BuildRoomSystem(RoomManager.DefaultRoomName, $"{username} joined"));
                BroadcastRoomUserList(RoomManager.DefaultRoomName);
            }

            SendTo(handler, PacketBuilder.BuildRoomList(_rooms.GetRoomNames()));

            OnStatusChanged?.Invoke($"{username} đã đăng nhập");
            OnUserListChanged?.Invoke(string.Join(", ", GetOnlineUsernames()));
        }

        private void RemoveClient(ClientHandler handler)
        {
            string endpoint = "unknown";
            string username = handler.Username;
            try { endpoint = handler.Socket.RemoteEndPoint?.ToString(); } catch { }
            if (!string.IsNullOrEmpty(handler.CurrentRoom))
            {
                ChatRoom room = FindRoom(handler.CurrentRoom);

                if (room != null)
                {
                    lock (room.Members)
                    {
                        room.Members.Remove(handler);
                        if (room.Members.Count == 0 && room.Owner != "SYSTEM")
                        {
                            lock (_rooms)
                            {
                                _rooms.Remove(room);
                            }
                        }
                        BroadcastRoomList();
                    }
                    BroadcastRoomSystem(room, $"{handler.Username} disconnected");
                }
            }
            handler.Close();
            lock (_clients)
                _clients.Remove(handler);

            OnClientDisconnected?.Invoke(endpoint);
            OnStatusChanged?.Invoke($"{_clients.Count} client(s) đang kết nối");

            if (!string.IsNullOrEmpty(username))
            {
                OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> Disconnected");

                string leaveMsg = PacketBuilder.BuildSystem($"{username} đã thoát");
                BroadcastToLoggedIn(leaveMsg);

                if (!string.IsNullOrEmpty(currentRoom))
                {
                    BroadcastToRoom(currentRoom, PacketBuilder.BuildRoomSystem(currentRoom, $"{username} left"));
                    BroadcastToRoom(currentRoom, PacketBuilder.BuildRoomUserLeft(currentRoom, username));
                    BroadcastRoomUserList(currentRoom);
                }

                BroadcastUserList();

                OnUserListChanged?.Invoke(string.Join(", ", GetOnlineUsernames()));
            }
        }

        private void BroadcastUserList()
        {
            BroadcastToLoggedIn(PacketBuilder.BuildUserList(GetOnlineUserEntries()));
        }

        private static void ProcessReceiveBuffer(ClientHandler handler, Action<string> handleLine)
        {
            while (true)
            {
                int newlineIndex = IndexOfNewline(handler.ReceiveBuffer);
                if (newlineIndex < 0)
                    break;

                string line = handler.ReceiveBuffer.ToString(0, newlineIndex);
                handler.ReceiveBuffer.Remove(0, newlineIndex + 1);

                if (line.EndsWith("\r"))
                    line = line.Substring(0, line.Length - 1);

                if (line.Length == 0)
                    continue;

                handleLine(line);
            }
        }

        public List<string> GetOnlineUsernames()
        {
            lock (_clients)
                return _clients
                    .Where(c => c.IsLoggedIn)
                    .Select(c => c.Username)
                    .ToList();
        }

        public List<UserListEntry> GetOnlineUserEntries()
        {
            lock (_clients)
                return _clients
                    .Where(c => c.IsLoggedIn)
                    .Select(c => new UserListEntry(c.Username, c.AvatarBase64))
                    .ToList();
        }

        public void BroadcastToLoggedIn(string packet)
        {
            if (!packet.EndsWith("\n")) packet += "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(packet);

            lock (_clients)
            {
                foreach (var client in _clients.Where(c => c.IsLoggedIn).ToList())
                {
                    try { client.Send(buffer); }
                    catch { }
                }
            }
        }

        // Giai đoạn 3.6: chỉ gửi message tới các ClientSession thuộc Room đó, không broadcast toàn server
        private void BroadcastToRoom(string roomName, string packet)
        {
            if (!packet.EndsWith("\n")) packet += "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(packet);

            foreach (var client in _rooms.GetMembersOf(roomName))
            {
                try { client.Send(buffer); }
                catch { }
            }
        }

        private void SendTo(ClientHandler handler, string packet)
        {
            if (!packet.EndsWith("\n")) packet += "\n";
            try { handler.Send(Encoding.UTF8.GetBytes(packet)); }
            catch { }
        }

        private static int IndexOfNewline(StringBuilder builder)
        {
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] == '\n')
                    return i;
            }

            return -1;
        }
        private ChatRoom FindRoom(string roomName)
        {
            lock (_rooms)
            {
                return _rooms.FirstOrDefault(r =>
                    r.RoomName.Equals(
                        roomName,
                        StringComparison.OrdinalIgnoreCase));
            }
        }
        private List<string> GetRoomNames()
        {
            lock (_rooms)
            {
                return _rooms
                    .Select(r => r.RoomName)
                    .ToList();
            }
        }

        private void HandleCreateRoom(ClientHandler handler, string roomName, int maxUsers, bool isPrivate, string password)
        {
            lock (_rooms)
            {
                if (_rooms.Count >= MAX_ROOMS)
                {
                    SendTo(handler, PacketBuilder.BuildSystem("Đã đạt giới hạn số Room"));
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(roomName))
                return;

            lock (_rooms)
            {
                bool exists = _rooms.Any(r =>
                    r.RoomName.Equals(
                        roomName,
                        StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    SendTo(
                        handler,
                        PacketBuilder.BuildRoomExists(roomName));

                    return;
                }

                ChatRoom room = new ChatRoom
                {
                    RoomName = roomName,
                    Owner = handler.Username,
                    MaxUsers = maxUsers,
                    IsPrivate = isPrivate,
                    Password = password
                };
                _roomRepo.AddRoom(room);
                _rooms.Add(room);
                OnStatusChanged?.Invoke($"[CREATE] {handler.Username} tạo room {roomName} | Max={maxUsers} | Private={isPrivate}");
                BroadcastRoomList();

                SendTo(
                    handler,
                    PacketBuilder.BuildCreateRoomOk(roomName));

                OnStatusChanged?.Invoke($"[CREATE] {handler.Username} tạo room {roomName}");
            }
        }
        private void HandleJoinRoom(ClientHandler handler, string roomName, string password)
        {
            if (!string.IsNullOrEmpty(handler.CurrentRoom))
            {
                SendTo(handler, PacketBuilder.BuildSystem("Bạn đang ở phòng khác"));
                return;
            }
            ChatRoom room = FindRoom(roomName);
            if (room == null)
            {
                SendTo(handler, PacketBuilder.BuildSystem($"Room {roomName} không tồn tại"));
                return;
            }
            if (room.IsPrivate)
            {
                if (room.Password != password)
                {
                    SendTo(handler, PacketBuilder.BuildSystem("Sai mật khẩu phòng"));
                    return;
                }
            }
            if (room.Members.Count >= room.MaxUsers)
            {
                SendTo(handler, PacketBuilder.BuildSystem("Phòng đã đầy"));
                return;
            }

            lock (room.Members)
            {
                if (!room.Members.Contains(handler))
                {
                    room.Members.Add(handler);
                }
            }

            handler.CurrentRoom = roomName;
            OnStatusChanged?.Invoke($"[JOIN] {handler.Username} -> {roomName}");
            SendTo(handler, PacketBuilder.BuildJoinRoomOk(roomName));
            // Gửi lịch sử chat của phòng
            List<MessageModel> history = MessageRepository.GetMessages(roomName);
            foreach (MessageModel msg in history)
            {
                SendTo(handler, PacketBuilder.BuildRoomHistory(roomName, msg.Sender, msg.Content, msg.CreatedAt.ToString()));
            }
            SendRoomMembers(handler, room);
            BroadcastRoomSystem(room, $"{handler.Username} joined {roomName}");
            BroadcastRoomUserJoined(room, handler.Username);
            BroadcastRoomList();
        }
        private void HandleLeaveRoom(ClientHandler handler, string roomName)
        {
            ChatRoom room = FindRoom(roomName);
            if (room == null)
            {
                return;
            }

            lock (room.Members)
            {
                room.Members.Remove(handler);
            }
            BroadcastRoomUserLeft(room, handler.Username);
            BroadcastRoomList();
            handler.CurrentRoom = null;
            SendTo(handler, PacketBuilder.BuildLeaveRoomOk(roomName));
            BroadcastRoomSystem(room, $"{handler.Username} left {roomName}");
            OnStatusChanged?.Invoke($"[LEAVE] {handler.Username} <- {roomName}");
        }
        private void HandleRoomMessage(ClientHandler handler, string roomName, string message)
        {
            ChatRoom room = FindRoom(roomName);
            if (room == null)
                return;
            if (handler.CurrentRoom != roomName)
            {
                SendTo(handler, PacketBuilder.BuildSystem("Bạn chưa tham gia room này"));
                return;
            }
            // Lưu tin nhắn vào SQLite
            MessageRepository.SaveMessage(roomName, handler.Username, message);
            string packet = $"ROOM_MSG|{roomName}|{handler.Username}|{message}\n";

            byte[] buffer = Encoding.UTF8.GetBytes(packet);

            lock (room.Members)
            {
                foreach (var member in room.Members)
                {
                    try
                    {
                        member.Send(buffer);
                    }
                    catch
                    {
                    }
                }
            }
            OnMessageReceived?.Invoke($"[{roomName}] {handler.Username}: {message}");
        }
        private void SendRoomMembers(ClientHandler handler, ChatRoom room)
        {
            List<string> users;
            lock (room.Members)
            {
                users = room.Members
                    .Where(m => m.IsLoggedIn)
                    .Select(m => m.Username)
                    .ToList();
            }

            SendTo(
                handler,
                PacketBuilder.BuildRoomUsers(
                    room.RoomName,
                    users));
        }
        private void HandleGetRooms(ClientHandler handler)
        {
            List<ChatRoom> rooms;
            lock (_rooms)
            {
                rooms = _rooms.ToList();
            }
            SendTo(handler, PacketBuilder.BuildRoomList(rooms));
        }
        private void BroadcastRoomUserJoined(ChatRoom room, string username)
        {
            string packet = PacketBuilder.BuildRoomUserJoined(room.RoomName, username);
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            lock (room.Members)
            {
                foreach (var member in room.Members)
                {
                    try
                    {
                        member.Send(buffer);
                    }
                    catch
                    {
                    }
                }
            }
        }
        private void BroadcastRoomUserLeft(ChatRoom room, string username)
        {
            string packet = PacketBuilder.BuildRoomUserLeft(room.RoomName, username);
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            lock (room.Members)
            {
                foreach (var member in room.Members)
                {
                    try
                    {
                        member.Send(buffer);
                    }
                    catch
                    {
                    }
                }
            }
        }
        private void BroadcastRoomList()
        {
            List<ChatRoom> rooms;
            lock (_rooms)
            {
                rooms = _rooms.ToList();
            }
            string packet = PacketBuilder.BuildRoomList(rooms);
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    SendTo(client, packet);
                }
            }
        }
        private void BroadcastRoomSystem(ChatRoom room, string message)
        {
            string packet = PacketBuilder.BuildSystem(message);
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            lock (room.Members)
            {
                foreach (var member in room.Members)
                {
                    try
                    {
                        member.Send(buffer);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}

