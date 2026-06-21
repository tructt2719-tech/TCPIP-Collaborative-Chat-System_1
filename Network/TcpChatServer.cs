using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class TcpChatServer
    {
        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<string> OnUserListChanged;

        private static readonly UserStore SharedUserStore = new UserStore();
        private readonly MessageHistoryStore _history = new MessageHistoryStore();
        private readonly RoomManager _rooms = new RoomManager();

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

            switch (command)
            {
                case PacketTypes.Register:
                    HandleRegister(handler, parts);
                    return;

                case PacketTypes.Login:
                    HandleLogin(handler, parts);
                    return;

                case PacketTypes.Message:
                    HandleChatMessage(handler, parts);
                    return;

                case PacketTypes.ReplyMessage:
                    HandleReplyMessage(handler, parts);
                    return;

                case PacketTypes.ForwardMessage:
                    HandleForwardMessage(handler, parts);
                    return;

                case PacketTypes.EmojiReaction:
                    HandleEmojiReaction(handler, parts);
                    return;

                // ===== Giai đoạn 3: Multi-Room =====
                case PacketTypes.CreateRoom:
                    HandleCreateRoom(handler, parts);
                    return;

                case PacketTypes.JoinRoom:
                    HandleJoinRoom(handler, parts);
                    return;

                case PacketTypes.LeaveRoom:
                    HandleLeaveRoom(handler, parts);
                    return;

                case PacketTypes.GetRooms:
                    HandleGetRooms(handler);
                    return;

                case PacketTypes.RoomMessage:
                    HandleRoomMessage(handler, parts);
                    return;

                case PacketTypes.RoomReplyMessage:
                    HandleRoomReplyMessage(handler, parts);
                    return;

                case PacketTypes.RoomForwardMessage:
                    HandleRoomForwardMessage(handler, parts);
                    return;
            }
        }

        // ===== 2.5 Định tuyến tin nhắn người dùng: gắn MessageId + Sender, lưu history để Reply/Forward tra cứu =====
        // MESSAGE|content   (client gửi - chưa có id, server tự sinh)
        // hoặc MESSAGE|msgId|content  (nếu client tự sinh id trước)
        private void HandleChatMessage(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 2) return;

            string content;
            string clientMsgId = null;

            if (parts.Length >= 3)
            {
                // MESSAGE|msgId|content  (client đã có id sẵn)
                clientMsgId = parts[1];
                content = parts[2];
            }
            else
            {
                // MESSAGE|content
                content = parts[1];
            }

            handler.Status = "Online";

            var model = new MessageModel(handler.Username, content);
            if (!string.IsNullOrWhiteSpace(clientMsgId))
                model.MessageId = clientMsgId;

            _history.Add(model);

            string safePacket = PacketBuilder.BuildMessage(model.MessageId, handler.Username, content);
            string display = handler.Username + ": " + content;
            OnMessageReceived?.Invoke(display);
            BroadcastToLoggedIn(safePacket);
        }

        // RELY_MESSAGE|msgId|sender|content|replyToId|replyToSender|replyToPreview
        // Client chỉ cần gửi: RELY_MESSAGE|msgId|sender|content|replyToId  (replyToSender/preview server tự tra)
        private void HandleReplyMessage(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 5) return;

            handler.Status = "Online";

            string msgId = parts[1];
            string content = parts[3];
            string replyToId = parts[4];

            string replyToSender = parts.Length > 5 ? parts[5] : string.Empty;
            string replyToPreview = parts.Length > 6 ? parts[6] : string.Empty;

            // Tra trong history để lấy đúng sender/preview gốc, không tin tưởng hoàn toàn dữ liệu client gửi lên
            if (_history.TryGet(replyToId, out MessageModel original))
            {
                replyToSender = original.Sender;
                replyToPreview = MessageModel.MakePreview(original.Content);
            }

            var model = new MessageModel(handler.Username, content)
            {
                MessageId = string.IsNullOrWhiteSpace(msgId) ? Guid.NewGuid().ToString("N") : msgId,
                IsReply = true,
                ReplyToId = replyToId,
                ReplyToSender = replyToSender,
                ReplyToPreview = replyToPreview
            };
            _history.Add(model);

            string safePacket = PacketBuilder.BuildReplyMessage(
                model.MessageId, handler.Username, content, replyToId, replyToSender, replyToPreview);

            string display = $"{handler.Username} (trả lời {replyToSender}): {content}";
            OnMessageReceived?.Invoke(display);
            BroadcastToLoggedIn(safePacket);
        }

        // FORWARD_MESSAGE|msgId|sender|content|originalSender
        // Client chỉ cần gửi: FORWARD_MESSAGE|msgId|originalMessageId  -> server tra nội dung + người gửi gốc
        private void HandleForwardMessage(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 3) return;

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

            var model = new MessageModel(handler.Username, content)
            {
                MessageId = string.IsNullOrWhiteSpace(msgId) ? Guid.NewGuid().ToString("N") : msgId,
                IsForwarded = true,
                OriginalSender = originalSender
            };
            _history.Add(model);

            string safePacket = PacketBuilder.BuildForwardMessage(
                model.MessageId, handler.Username, content, originalSender);

            string display = $"{handler.Username} (chuyển tiếp từ {originalSender}): {content}";
            OnMessageReceived?.Invoke(display);
            BroadcastToLoggedIn(safePacket);
        }

        // Không-room (2.5): EMOJI_REACTION|msgId|reactor|emoji  (4 phần)
        // Room (Giai đoạn 3): EMOJI_REACTION|roomName|msgId|reactor|emoji  (5 phần)
        private void HandleEmojiReaction(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;

            if (parts.Length >= 5)
            {
                // Dạng room-scoped
                string roomName = parts[1];
                string messageId = parts[2];
                string emoji = parts[4];

                string packet = PacketBuilder.BuildRoomEmojiReactionBroadcast(roomName, messageId, handler.Username, emoji);
                OnMessageReceived?.Invoke($"[{roomName}] {handler.Username} đã react {emoji} vào {messageId}");

                if (_rooms.RoomExists(roomName))
                    BroadcastToRoom(roomName, packet);
                else
                    BroadcastToLoggedIn(packet);
                return;
            }

            if (parts.Length < 4) return;

            string nonRoomMessageId = parts[1];
            string nonRoomEmoji = parts[3];

            string safePacket = PacketBuilder.BuildEmojiReactionBroadcast(nonRoomMessageId, handler.Username, nonRoomEmoji);
            string display = $"{handler.Username} đã phản ứng với {nonRoomMessageId} bằng {nonRoomEmoji}";
            OnMessageReceived?.Invoke(display);
            BroadcastToLoggedIn(safePacket);
        }

        // ===================================================================
        // Giai đoạn 3: Multi-Room Architecture
        // ===================================================================

        // CREATE_ROOM|roomName
        private void HandleCreateRoom(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 2) return;

            string roomName = parts[1].Trim();
            var result = _rooms.CreateRoom(roomName);

            switch (result)
            {
                case RoomManager.CreateResult.Created:
                    SendTo(handler, PacketBuilder.BuildCreateRoomOk(roomName));
                    BroadcastToLoggedIn(PacketBuilder.BuildRoomCreated(roomName));
                    OnMessageReceived?.Invoke($"[ROOM] {handler.Username} đã tạo phòng '{roomName}'");
                    break;

                case RoomManager.CreateResult.AlreadyExists:
                    SendTo(handler, PacketBuilder.BuildError(PacketTypes.ErrorRoomExists, roomName));
                    break;

                case RoomManager.CreateResult.InvalidName:
                    SendTo(handler, PacketBuilder.BuildError("ROOM_INVALID_NAME", roomName));
                    break;
            }
        }

        // JOIN_ROOM|roomName
        private void HandleJoinRoom(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 2) return;

            string roomName = parts[1].Trim();
            string previousRoom = handler.CurrentRoom;

            var result = _rooms.Join(handler, roomName);

            switch (result)
            {
                case RoomManager.JoinResult.Joined:
                    // 3.5 + 3.8: báo cho các thành viên còn lại của room CŨ rằng user đã rời
                    if (!string.IsNullOrEmpty(previousRoom) &&
                        !previousRoom.Equals(roomName, StringComparison.OrdinalIgnoreCase))
                    {
                        BroadcastToRoom(previousRoom,
                            PacketBuilder.BuildRoomSystem(previousRoom, $"{handler.Username} left"));
                        BroadcastToRoom(previousRoom, PacketBuilder.BuildRoomUserLeft(previousRoom, handler.Username));
                        BroadcastRoomUserList(previousRoom);
                    }

                    SendTo(handler, PacketBuilder.BuildJoinRoomOk(roomName));

                    BroadcastToRoom(roomName, PacketBuilder.BuildRoomSystem(roomName, $"{handler.Username} joined"));
                    BroadcastToRoom(roomName, PacketBuilder.BuildRoomUserJoined(roomName, handler.Username));
                    BroadcastRoomUserList(roomName);

                    OnMessageReceived?.Invoke($"[ROOM] {handler.Username} -> {roomName}");
                    break;

                case RoomManager.JoinResult.RoomNotFound:
                    SendTo(handler, PacketBuilder.BuildError(PacketTypes.ErrorRoomNotFound, roomName));
                    break;

                case RoomManager.JoinResult.NotLoggedIn:
                    SendTo(handler, PacketBuilder.BuildLoginFail("Bạn chưa đăng nhập"));
                    break;
            }
        }

        // LEAVE_ROOM|roomName
        private void HandleLeaveRoom(ClientHandler handler, string[] parts)
        {
            if (!RequireLoggedIn(handler)) return;
            if (parts.Length < 2) return;

            string roomName = parts[1].Trim();
            var result = _rooms.Leave(handler, roomName);

            if (result == RoomManager.LeaveResult.Left)
            {
                SendTo(handler, PacketBuilder.BuildLeaveRoomOk(roomName));
                BroadcastToRoom(roomName, PacketBuilder.BuildRoomSystem(roomName, $"{handler.Username} left"));
                BroadcastToRoom(roomName, PacketBuilder.BuildRoomUserLeft(roomName, handler.Username));
                BroadcastRoomUserList(roomName);
                OnMessageReceived?.Invoke($"[ROOM] {handler.Username} left {roomName}");
            }
            else
            {
                SendTo(handler, PacketBuilder.BuildError(PacketTypes.ErrorRoomNotFound, roomName));
            }
        }

        // GET_ROOMS
        private void HandleGetRooms(ClientHandler handler)
        {
            SendTo(handler, PacketBuilder.BuildRoomList(_rooms.GetRoomNames()));
        }

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

            string currentRoom = _rooms.LeaveCurrentRoom(handler);

            handler.Status = "Disconnected";
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
    }
}
