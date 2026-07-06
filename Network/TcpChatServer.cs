using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPIP_Collaborative_Chat_System.Shared;
<<<<<<< Updated upstream
=======
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Database;
using TCPIP_Collaborative_Chat_System.Services.Security;
>>>>>>> Stashed changes

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class TcpChatServer
    {
        // Events to notify UI - no direct UI dependency
        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<string> OnUserListChanged;


        private Socket _serverSocket;
        private readonly List<ClientHandler> _clients = new List<ClientHandler>();

        public void Start(int port)
        {
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                _serverSocket.Listen(10);
                _serverSocket.BeginAccept(HandleConnection, null);
                OnStatusChanged?.Invoke($"Server chạy ở port {port}");
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Lỗi server: " + ex.Message);
            }
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
<<<<<<< Updated upstream
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            List<ClientHandler> disconnected = new List<ClientHandler>();
=======
            byte[] buffer = EncryptionService.Encrypt( Encoding.UTF8.GetBytes(packet));
>>>>>>> Stashed changes

            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    try
                    {
                        if (client.Socket.Connected)
                        {
                            client.Send(buffer);
                        }
                    }
                    catch
                    {
                        disconnected.Add(client);
                    }
                }
            }
            foreach (var client in disconnected)
            {
                RemoveClient(client);
            }
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

                // Continue accepting next client
                _serverSocket.BeginAccept(HandleConnection, null);

                // Start receiving from this client
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
                byte[] encrypted = new byte[size];
                Array.Copy(handler.Buffer, encrypted, size);
                byte[] decrypted;
                try
                {
                    decrypted = EncryptionService.Decrypt(encrypted);
                }
                catch
                {
                    SendTo(handler, "ERROR|DECRYPT_FAILED|Cannot decrypt packet");
                    return;
                }
                string chunk = Encoding.UTF8.GetString(decrypted);
                handler.ReceiveBuffer.Append(chunk);

                ProcessReceiveBuffer(handler, line =>
                {
                    string[] parts = PacketParser.Parse(line);
                    if (parts.Length == 0) return;

                    string command = parts[0];
<<<<<<< Updated upstream
=======
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
                    if (command == PacketTypes.DeleteRoom && parts.Length >= 2)
                    {
                        if (!handler.IsLoggedIn) return;
                        HandleDeleteRoom(handler, parts[1]);
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

                        string passwordHash = PasswordHasher.Hash(password);
                        UserRepository.AddUser( username, passwordHash);

                        SendTo(handler,
                            PacketBuilder.BuildRegisterOk(username));

                        OnStatusChanged?.Invoke($"Đăng ký thành công: {username}");

                        return;
                    }
>>>>>>> Stashed changes

                    // LOGIN|Alice
                    if (command == PacketTypes.Login && parts.Length >= 2)
                    {
                        string username = parts[1].Trim();
<<<<<<< Updated upstream
=======
                        string passwordHash = parts[2];
>>>>>>> Stashed changes

                        // Kiểm tra username rỗng
                        if (string.IsNullOrWhiteSpace(username))
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Username không được rỗng"));
                            return;
                        }
                        // Độ dài (1–20 ký tự)
                        if (username.Length < 1 || username.Length > 20)
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Username phải từ 1-20 ký tự"));
                            return;
                        }

                        // Ký tự không hợp lệ (cấm ký tự phá packet)
                        if (username.Contains("|") || username.Contains("\n") || username.Contains("\r"))
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Username chứa ký tự không hợp lệ"));
                            return;
                        }

                        // Kiểm tra trùng username
                        bool isDuplicate;
                        lock (_clients)
                            isDuplicate = _clients.Any(c => c.IsLoggedIn &&
                                          c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                        if (isDuplicate)
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Username đã tồn tại"));
                            return;
                        }

                        // Đăng ký username thành công
                        handler.Username = username;
                        handler.Status = "LoggedIn";
                        OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> LoggedIn");
                        SendTo(handler, PacketBuilder.BuildLoginOk(username));
                        handler.Status = "Online";
                        OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> Online");

                        // Thông báo cho tất cả
                        string joinMsg = PacketBuilder.BuildSystem($"{username} đã tham gia");
                        BroadcastToLoggedIn(joinMsg);

                        // Gửi danh sách user online cho tất cả
                        string userListPacket = PacketBuilder.BuildUserList(GetOnlineUsernames());
                        BroadcastToLoggedIn(userListPacket);

                        OnStatusChanged?.Invoke($"{username} đã đăng nhập");
                        OnUserListChanged?.Invoke(string.Join(", ", GetOnlineUsernames()));
                        return;
                    }

                    // MESSAGE — chỉ xử lý nếu đã login
                    if (command == PacketTypes.Message && parts.Length >= 3)
                    {
                        if (!handler.IsLoggedIn)
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Bạn chưa đăng nhập"));
                            return;
                        }
                        handler.Status = "Online";
                        OnMessageReceived?.Invoke($"[STATUS] {handler.Username} -> {handler.Status}");

                        // Dùng username từ server, không tin username client gửi lên
                        string safePacket = PacketBuilder.BuildMessage(handler.Username, parts[2]);
                        string display = handler.Username + ": " + parts[2];
                        OnMessageReceived?.Invoke(display);
                        Broadcast(safePacket);
                    }
                });

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

        private void RemoveClient(ClientHandler handler)
        {
            string endpoint = "unknown";
            string username = handler.Username;
            try { endpoint = handler.Socket.RemoteEndPoint?.ToString(); } catch { }

            handler.Status = "Disconnected";
            handler.Close();
            lock (_clients)
                _clients.Remove(handler);

            OnClientDisconnected?.Invoke(endpoint);
            OnStatusChanged?.Invoke($"{_clients.Count} client(s) đang kết nối");
            // ghi nhật kí lifecycle
            if (!string.IsNullOrEmpty(username))
            {
                OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> Disconnected");

                // thông báo user rời đi
                string leaveMsg = PacketBuilder.BuildSystem($"{username} đã thoát");

                BroadcastToLoggedIn(leaveMsg);

                // cập nhật lại danh sách online
                string userListPacket = PacketBuilder.BuildUserList(GetOnlineUsernames());
                BroadcastToLoggedIn(userListPacket);

                // update UI server
                OnUserListChanged?.Invoke(string.Join(", ", GetOnlineUsernames()));
            }
        }

        private static void ProcessReceiveBuffer(ClientHandler handler, Action<string> handleLine)
        {
            while (true)
            {
                int newlineIndex = IndexOfNewline(handler.ReceiveBuffer);
                if (newlineIndex < 0)
                {
                    break;
                }

                string line = handler.ReceiveBuffer.ToString(0, newlineIndex);
                handler.ReceiveBuffer.Remove(0, newlineIndex + 1);

                if (line.EndsWith("\r"))
                {
                    line = line.Substring(0, line.Length - 1);
                }

                if (line.Length == 0)
                {
                    continue;
                }

                handleLine(line);
            }
        }

        // method lấy danh sách username online
        public List<string> GetOnlineUsernames()
        {
            lock (_clients)
                return _clients
                    .Where(c => c.IsLoggedIn)
                    .Select(c => c.Username)
                    .ToList();
        }
        //method broadcast chỉ cho loggedin users
        public void BroadcastToLoggedIn(string packet)
        {
            if (!packet.EndsWith("\n")) packet += "\n";
            byte[] buffer = EncryptionService.Encrypt( Encoding.UTF8.GetBytes(packet));

            lock (_clients)
            {
                foreach (var client in _clients.Where(c => c.IsLoggedIn).ToList())
                {
                    try { client.Send(buffer); }
                    catch { }
                }
            }
        }

        // method gửi cho 1 client
        private void SendTo(ClientHandler handler, string packet)
        {
            if (!packet.EndsWith("\n")) packet += "\n";
            try {
                byte[] data = Encoding.UTF8.GetBytes(packet);
                data = EncryptionService.Encrypt(data);
                handler.Send(data);
            }
            catch { }
        }
        private static int IndexOfNewline(StringBuilder builder)
        {
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] == '\n')
                {
                    return i;
                }
            }

            return -1;
        }
<<<<<<< Updated upstream
=======
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
                    SendTo(handler, PacketBuilder.BuildSystem($"Phòng '{roomName}' đã tồn tại"));

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

        private void HandleDeleteRoom(ClientHandler handler, string roomName)
        {
            ChatRoom room = FindRoom(roomName);

            // Kiểm tra phòng tồn tại
            if (room == null)
            {
                SendTo(handler, PacketBuilder.BuildDeleteRoomFail($"Phòng '{roomName}' không tồn tại"));
                return;
            }

            if (room.Owner == "SYSTEM")
            {
                SendTo(handler, PacketBuilder.BuildDeleteRoomFail("Không thể xóa phòng hệ thống"));
                return;
            }

            // Chỉ chủ phòng mới được xóa
            if (!room.Owner.Equals(handler.Username, StringComparison.OrdinalIgnoreCase))
            {
                SendTo(handler,
                    PacketBuilder.BuildDeleteRoomFail($"Chỉ chủ phòng ({room.Owner}) mới được xóa"));
                return;
            }

            List<ClientHandler> members;
            lock (room.Members) { members = room.Members.ToList(); }

            // Thông báo ROOM_DELETED cho tất cả thành viên đang ở phòng
            byte[] deletedBuf = EncryptionService.Encrypt( Encoding.UTF8.GetBytes( PacketBuilder.BuildRoomDeleted( roomName, handler.Username)));
            foreach (var m in members)
            {
                m.CurrentRoom = null;           
                try { m.Send(deletedBuf); } catch { }
            }

            // Xóa khỏi danh sách rooms
            lock (_rooms) _rooms.Remove(room);

            try { _roomRepo.DeleteRoom(roomName); }
            catch (Exception ex) { OnStatusChanged?.Invoke($"[DELETE][DB ERROR] {ex.Message}"); }

            // Xác nhận cho chủ phòng
            SendTo(handler, PacketBuilder.BuildDeleteRoomOk(roomName));

            // Thông báo toàn server
            BroadcastToLoggedIn(PacketBuilder.BuildSystem(
                $"Phòng '{roomName}' đã bị xóa bởi {handler.Username}"));
            BroadcastRoomList();

            OnStatusChanged?.Invoke($"[DELETE] {handler.Username} xóa phòng '{roomName}'");
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
            
            List<MessageModel> history = MessageRepository.GetMessages(roomName);
            foreach (MessageModel msg in history)
            {
                SendTo(handler, PacketBuilder.BuildRoomHistory(roomName, msg.Sender, msg.Content, msg.CreatedAt.ToString()));
            }
            SendRoomMembers(handler, room);
            BroadcastRoomSystem(room, $"{handler.Username} joined {roomName}");
            BroadcastRoomUserJoined(room, handler.Username);
            BroadcastRoomMembersToAll(room);
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
            handler.CurrentRoom = null;
            BroadcastRoomUserLeft(room, handler.Username);
            BroadcastRoomSystem(room, $"{handler.Username} left {roomName}");
            BroadcastRoomList();
            BroadcastRoomMembersToAll(room);
            SendTo(handler, PacketBuilder.BuildLeaveRoomOk(roomName));
            HandleGetRooms(handler);
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

            byte[] buffer = EncryptionService.Encrypt( Encoding.UTF8.GetBytes(packet));

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
            byte[] buffer = EncryptionService.Encrypt( Encoding.UTF8.GetBytes(packet));
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
            byte[] buffer = EncryptionService.Encrypt(Encoding.UTF8.GetBytes(packet));
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
            byte[] buffer = EncryptionService.Encrypt(Encoding.UTF8.GetBytes(packet));
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
        private void BroadcastRoomMembersToAll(ChatRoom room)
        {
            List<string> users;
            lock (room.Members)
            {
                users = room.Members.Where(m => m.IsLoggedIn)
                    .Select(m => m.Username).ToList();
            }
            string packet = PacketBuilder.BuildRoomUsers(room.RoomName, users);
            byte[] buffer = EncryptionService.Encrypt(Encoding.UTF8.GetBytes(packet));
            lock (room.Members)
            {
                foreach (var m in room.Members)
                    try { m.Send(buffer); } catch { }
            }
        }
>>>>>>> Stashed changes
    }
}
