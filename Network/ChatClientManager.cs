using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class OnlineUserInfo
    {
        public string Username { get; set; }
        public string AvatarBase64 { get; set; }
    }

    /// <summary>Một message hiển thị trên Chat Panel - đủ dữ liệu để hỗ trợ Reply/Forward/Emoji reaction.</summary>
    public class ChatMessageInfo
    {
        public string MessageId { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public bool IsReply { get; set; }
        public string ReplyToId { get; set; }
        public string ReplyToSender { get; set; }
        public string ReplyToPreview { get; set; }
        public bool IsForwarded { get; set; }
        public string OriginalSender { get; set; }

        /// <summary>Tên Room mà message này thuộc về (Giai đoạn 3.6). Rỗng/null nếu là message không-theo-room (2.5).</summary>
        public string RoomName { get; set; }

        public string ToDisplayText()
        {
            if (IsReply)
                return $"{Sender} (trả lời {ReplyToSender}: \"{ReplyToPreview}\"): {Content}";
            if (IsForwarded)
                return $"{Sender} (chuyển tiếp từ {OriginalSender}): {Content}";
            return $"{Sender}: {Content}";
        }
    }

    public class ChatClientManager
    {
        public string Username { get; private set; }
        public string AvatarBase64 { get; private set; } = string.Empty;
        public bool IsLoggedIn => Username != null;

        /// <summary>Tên Room hiện tại mà client này đang ở (Giai đoạn 3.7), cập nhật khi nhận JOIN_ROOM_OK.</summary>
        public string CurrentRoom { get; private set; }

        private readonly List<string> _knownRooms = new List<string>();
        private readonly object _knownRoomsLock = new object();

        public event Action<string> OnStatusChanged;

        /// <summary>Bắn ra mỗi khi có message mới hiển thị được trên Chat Panel (chat/reply/forward).</summary>
        public event Action<ChatMessageInfo> OnChatMessage;

        /// <summary>
        /// Bắn ra cho MỌI packet thô (raw line) nhận được từ Server, trước khi parse theo command.
        /// Dùng để debug/log - không bắt buộc phải lắng nghe (UI không cần dùng nếu không muốn).
        /// </summary>
        public event Action<string> OnMessageReceived;

        public event Action OnDisconnected;
        public event Action<string> OnLoginResult;
        public event Action<string> OnSystemMessage;
        public event Action<List<string>> OnUserListUpdated;
        public event Action<string> OnRoomMessage;
        public event Action<string> OnRoomUsers;
        public event Action<string> OnRoomUserJoined;
        public event Action<string> OnRoomUserLeft;
        public event Action<string> OnRoomHistory;
        public event Action<List<string>> OnRoomListReceived;


        private Socket _socket;
        private readonly byte[] _buffer = new byte[1024];
        private readonly StringBuilder _receiveBuffer = new StringBuilder();

        private bool _isConnecting;
        private bool _waitingForRegister;
        private bool _registerSuccess;
        private string _registerMessage;
        private readonly ManualResetEventSlim _registerWaitHandle = new ManualResetEventSlim(false);

        private bool _waitingForLogin;
        private bool _loginSuccess;
        private string _loginMessage;
        private readonly ManualResetEventSlim _loginWaitHandle = new ManualResetEventSlim(false);

        public bool IsConnected => _socket != null && _socket.Connected;

        public void Connect(string ip, int port)
        {
            if (_isConnecting || IsConnected)
            {
                OnStatusChanged?.Invoke("Client đã kết nối hoặc đang kết nối.");
                return;
            }

            try
            {
                _isConnecting = true;
                Username = null;

                Socket socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _socket = socket;
                socket.BeginConnect(endpoint, HandleConnection, socket);
            }
            catch (Exception ex)
            {
                _isConnecting = false;
                _socket = null;
                OnStatusChanged?.Invoke("Kết nối thất bại: " + ex.Message);
            }
        }

        /// <summary>
        /// Gửi packet LOGIN trên socket đang giữ (dùng sau khi Connect() đã thành công bất đồng bộ,
        /// ví dụ trong TcpChatClientForm khi OnStatusChanged báo "Kết nối thành công.").
        /// Kết quả LOGIN_OK/LOGIN_FAIL trả về qua event OnLoginResult, không block luồng gọi.
        /// </summary>
        public void Login(string username, string password)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }

            try
            {
                string packet = PacketBuilder.BuildLogin(username, password);
                socket.Send(Encoding.UTF8.GetBytes(packet));
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi LOGIN thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public bool TryLogin(
            string ip,
            int port,
            string username,
            string password,
            out string error,
            int timeoutMs = 5000)
        {
            error = null;
            const int maxAttempts = 2;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Socket socket = null;

                try
                {
                    _receiveBuffer.Clear();

                    socket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);

                    socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                    _waitingForLogin = true;
                    _loginSuccess = false;
                    _loginMessage = null;
                    _loginWaitHandle.Reset();
                    Username = null;

                    _socket = socket;
                    string packet = PacketBuilder.BuildLogin(username, password);
                    socket.Send(Encoding.UTF8.GetBytes(packet));

                    socket.BeginReceive(
                        _buffer,
                        0,
                        _buffer.Length,
                        SocketFlags.None,
                        HandleDataReceived,
                        socket);

                    if (!_loginWaitHandle.Wait(timeoutMs))
                    {
                        if (attempt < maxAttempts)
                            continue;

                        error = $"Hết thời gian chờ phản hồi từ server ({timeoutMs}ms). " +
                                "Kiểm tra server đang chạy và port đúng.";
                        return false;
                    }

                    error = _loginSuccess ? null : _loginMessage;
                    return _loginSuccess;
                }
                catch (Exception ex)
                {
                    if (attempt < maxAttempts)
                        continue;

                    error = "Đăng nhập thất bại: " + ex.Message;
                    return false;
                }
                finally
                {
                    _waitingForLogin = false;
                    Username = null;

                    if (_socket == socket)
                        _socket = null;

                    if (socket != null)
                        SafeClose(socket);
                }
            }

            error = "Đăng nhập thất bại";
            return false;
        }

        public bool TryRegister(
            string ip,
            int port,
            string username,
            string password,
            string email,
            string avatarBase64,
            out string error,
            int timeoutMs = 5000)
        {
            error = null;
            Socket socket = null;

            try
            {
                _receiveBuffer.Clear();

                socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                _waitingForRegister = true;
                _registerSuccess = false;
                _registerMessage = null;
                _registerWaitHandle.Reset();

                _socket = socket;
                string packet = PacketBuilder.BuildRegister(username, password, email, avatarBase64 ?? string.Empty);
                socket.Send(Encoding.UTF8.GetBytes(packet));

                socket.BeginReceive(
                    _buffer,
                    0,
                    _buffer.Length,
                    SocketFlags.None,
                    HandleDataReceived,
                    socket);

                if (!_registerWaitHandle.Wait(timeoutMs))
                {
                    error = "Hết thời gian chờ phản hồi từ server";
                    return false;
                }

                error = _registerSuccess ? null : _registerMessage;
                return _registerSuccess;
            }
            catch (Exception ex)
            {
                error = "Đăng ký thất bại: " + ex.Message;
                return false;
            }
            finally
            {
                _waitingForRegister = false;

                if (_socket == socket)
                    _socket = null;

                if (socket != null)
                    SafeClose(socket);
            }
        }

        /// <summary>Gửi tin nhắn chat thường. Trả về MessageId mà client tự sinh (để có thể track local nếu cần).</summary>
        public string Send(string senderName, string message)
        {
            Socket socket = _socket;

            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(message))
                return null;

            string messageId = Guid.NewGuid().ToString("N");

            try
            {
                string packet = $"MESSAGE|{messageId}|{message}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
                return messageId;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn thất bại: " + ex.Message);
                HandleDisconnected(socket);
                return null;
            }
        }

        public void SendReply(string message, string replyToId)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(replyToId))
                return;

            try
            {
                string messageId = Guid.NewGuid().ToString("N");
                string packet = $"RELY_MESSAGE|{messageId}|{Username}|{message}|{replyToId}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn trả lời thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        /// <summary>Gửi forward. Chỉ cần originalMessageId - Server tra ra content + originalSender.</summary>
        public void SendForward(string originalMessageId)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(originalMessageId))
                return;

            try
            {
                string messageId = Guid.NewGuid().ToString("N");
                string packet = $"FORWARD_MESSAGE|{messageId}|{originalMessageId}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn chuyển tiếp thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public void SendEmojiReaction(string messageId, string emoji)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(emoji))
                return;

            try
            {
                string packet = PacketBuilder.BuildEmojiReaction(messageId, Username, emoji);
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi phản ứng emoji thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        // ===================================================================
        // Giai đoạn 3: Multi-Room Architecture
        // ===================================================================

        public void CreateRoom(string roomName)
        {
            SendRaw(PacketBuilder.BuildCreateRoom(roomName), "Tạo room thất bại");
        }

        public void JoinRoom(string roomName)
        {
            SendRaw(PacketBuilder.BuildJoinRoom(roomName), "Tham gia room thất bại");
        }

        public void LeaveRoom(string roomName)
        {
            SendRaw(PacketBuilder.BuildLeaveRoom(roomName), "Rời room thất bại");
        }

        public void GetRooms()
        {
            SendRaw(PacketBuilder.BuildGetRooms(), "Lấy danh sách room thất bại");
        }

        /// <summary>Gửi tin nhắn chat vào room hiện tại (Giai đoạn 3.6). Trả về MessageId tự sinh.</summary>
        public string SendRoomMessage(string roomName, string message)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(message))
                return null;

            string messageId = Guid.NewGuid().ToString("N");

            try
            {
                string packet = $"{PacketTypes.RoomMessage}|{roomName}|{messageId}|{message}\n";
                socket.Send(Encoding.UTF8.GetBytes(packet));
                return messageId;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn vào room thất bại: " + ex.Message);
                HandleDisconnected(socket);
                return null;
            }
        }

        /// <summary>Reply trong room. Chỉ cần replyToId - Server tự tra replyToSender/preview.</summary>
        public void SendRoomReply(string roomName, string message, string replyToId)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(message) ||
                string.IsNullOrWhiteSpace(replyToId))
                return;

            try
            {
                string messageId = Guid.NewGuid().ToString("N");
                string packet = $"{PacketTypes.RoomReplyMessage}|{roomName}|{messageId}|{message}|{replyToId}\n";
                socket.Send(Encoding.UTF8.GetBytes(packet));
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn trả lời (room) thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public void SendRoomMessage(string roomName, string message)
        {
            if (!IsConnected)
                return;
            string packet = $"ROOM_MSG|{roomName}|{message}\n";
            _socket.Send(Encoding.UTF8.GetBytes(packet));
        }

        public void Disconnect()
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(originalMessageId))
                return;

            try
            {
                string messageId = Guid.NewGuid().ToString("N");
                string packet = $"{PacketTypes.RoomForwardMessage}|{roomName}|{messageId}|{originalMessageId}\n";
                socket.Send(Encoding.UTF8.GetBytes(packet));
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn chuyển tiếp (room) thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public void SendRoomEmojiReaction(string roomName, string messageId, string emoji)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(messageId) ||
                string.IsNullOrWhiteSpace(emoji))
                return;

            try
            {
                string packet = PacketBuilder.BuildRoomEmojiReaction(roomName, messageId, Username, emoji);
                socket.Send(Encoding.UTF8.GetBytes(packet));
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi phản ứng emoji (room) thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        private void SendRaw(string packet, string errorPrefix)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }

            try
            {
                socket.Send(Encoding.UTF8.GetBytes(packet));
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"{errorPrefix}: {ex.Message}");
                HandleDisconnected(socket);
            }
        }

        public void Disconnect()
        {
            Socket socket = _socket;

            if (socket == null)
                return;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            HandleDisconnected(socket);
        }

        private void HandleConnection(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;

            if (socket == null)
            {
                _isConnecting = false;
                OnStatusChanged?.Invoke("Kết nối thất bại: Socket không hợp lệ.");
                return;
            }

            try
            {
                socket.EndConnect(ar);

                if (_socket != socket)
                {
                    SafeClose(socket);
                    return;
                }

                _isConnecting = false;
                OnStatusChanged?.Invoke("Kết nối thành công.");

                socket.BeginReceive(
                    _buffer,
                    0,
                    _buffer.Length,
                    SocketFlags.None,
                    HandleDataReceived,
                    socket);
            }
            catch (Exception ex)
            {
                _isConnecting = false;

                if (_socket == socket)
                    _socket = null;

                SafeClose(socket);
                OnStatusChanged?.Invoke("Kết nối thất bại: " + ex.Message);
            }
        }

        private void HandleDataReceived(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;

            if (socket == null)
                return;

            try
            {
                if (_socket != socket && !_waitingForRegister && !_waitingForLogin)
                {
                    SafeClose(socket);
                    return;
                }

                int size = socket.EndReceive(ar);

                if (size == 0)
                {
                    if (_waitingForRegister)
                    {
                        _registerMessage = "Server đã đóng kết nối";
                        _registerSuccess = false;
                        _registerWaitHandle.Set();
                    }
                    else if (_waitingForLogin)
                    {
                        _loginMessage = "Server đã đóng kết nối";
                        _loginSuccess = false;
                        _loginWaitHandle.Set();
                    }
                    else
                    {
                        HandleDisconnected(socket);
                    }
                    return;
                }

                string chunk = Encoding.UTF8.GetString(_buffer, 0, size);
                _receiveBuffer.Append(chunk);

                ProcessReceiveBuffer(HandleLine);

                if (_waitingForRegister || _waitingForLogin)
                    return;

                if (_socket == socket && socket.Connected)
                {
                    socket.BeginReceive(
                        _buffer,
                        0,
                        _buffer.Length,
                        SocketFlags.None,
                        HandleDataReceived,
                        socket);
                }
            }
            catch
            {
                if (_waitingForRegister)
                {
                    _registerMessage = "Lỗi khi nhận phản hồi từ server";
                    _registerSuccess = false;
                    _registerWaitHandle.Set();
                }
                else if (_waitingForLogin)
                {
                    _loginMessage = "Lỗi khi nhận phản hồi từ server";
                    _loginSuccess = false;
                    _loginWaitHandle.Set();
                }
                else
                {
                    HandleDisconnected(socket);
                }
            }
        }

        private void HandleLine(string line)
        {
            OnMessageReceived?.Invoke(line);

            string[] parts = PacketParser.Parse(line);
            if (parts.Length == 0) return;

            string command = parts[0].Trim();

            switch (command)
            {
                case PacketTypes.LoginOk:
                    if (_waitingForLogin)
                    {
                        _loginSuccess = true;
                        _loginMessage = parts.Length > 1 ? parts[1] : "OK";
                        _loginWaitHandle.Set();
                    }
                    else
                    {
                        Username = parts.Length > 1 ? parts[1] : null;
                        AvatarBase64 = parts.Length > 2 ? parts[2] : string.Empty;
                        OnLoginResult?.Invoke("OK:" + (parts.Length > 1 ? parts[1] : ""));
                    }
                    break;

                case PacketTypes.LoginFail:
                    if (_waitingForLogin)
                    {
                        _loginSuccess = false;
                        _loginMessage = parts.Length > 1 ? parts[1] : "Đăng nhập thất bại";
                        _loginWaitHandle.Set();
                    }
                    else
                    {
                        Username = null;
                        OnLoginResult?.Invoke("FAIL:" + (parts.Length > 1 ? parts[1] : ""));
                    }
                    break;

                case PacketTypes.RegisterOk:
                    if (_waitingForRegister)
                    {
                        _registerSuccess = true;
                        _registerMessage = parts.Length > 1 ? parts[1] : "OK";
                        _registerWaitHandle.Set();
                    }
                    break;

                case PacketTypes.RegisterFail:
                    if (_waitingForRegister)
                    {
                        _registerSuccess = false;
                        _registerMessage = parts.Length > 1 ? parts[1] : "Đăng ký thất bại";
                        _registerWaitHandle.Set();
                    }
                    break;

                case PacketTypes.System:
                    OnSystemMessage?.Invoke(parts.Length > 1 ? parts[1] : "");
                    break;

                case PacketTypes.UserList:
                    {
                        var users = parts.Skip(1)
                            .Where(p => !string.IsNullOrEmpty(p))
                            .Select(ParseUserListEntry)
                            .ToList();
                        OnUserListUpdated?.Invoke(users);
                    }
                    break;

                case PacketTypes.Message:
                    // MESSAGE|msgId|sender|content
                    if (parts.Length >= 4)
                    {
                        OnChatMessage?.Invoke(new ChatMessageInfo
                        {
                            MessageId = parts[1],
                            Sender = parts[2],
                            Content = parts[3]
                        });
                    }
                    break;

                        case "MESSAGE":
                            if (parts.Length >= 3)
                                OnMessageReceived?.Invoke(parts[1] + ": " + parts[2]);
                            break;

                        case "ROOM_MSG":
                            if (parts.Length >= 4)
                            {
                                OnRoomMessage?.Invoke(
                                    "[" + parts[1] + "] "
                                    + parts[2]
                                    + ": "
                                    + parts[3]);
                            }
                            break;

                        case "ROOM_HISTORY":
                            {
                                if (parts.Length >= 5)
                                {
                                    OnRoomHistory?.Invoke(
                                        "[" + parts[4] + "] "
                                        + parts[2]
                                        + ": "
                                        + parts[3]);
                                }
                                break;
                            }

                        case "ROOM_USERS":
                            if (parts.Length >= 2)
                            {
                                OnRoomUsers?.Invoke(string.Join(", ", parts.Skip(2)));
                            }
                            break;

                        case "ROOM_USER_JOINED":
                            if (parts.Length >= 3)
                            {
                                OnRoomUserJoined?.Invoke($"{parts[2]} joined {parts[1]}");
                            }
                            break;

                        case "ROOM_USER_LEFT":
                            if (parts.Length >= 3)
                            {
                                OnRoomUserLeft?.Invoke($"{parts[2]} left {parts[1]}");
                            }
                            break;

                        case "LEAVE_ROOM_OK":
                            {
                                if (parts.Length >= 2)
                                {
                                    OnMessageReceived?.Invoke(
                                        $"Đã rời phòng {parts[1]}");
                                }
                            }
                            break;

                        case "ROOM_LIST":
                            {
                                List<string> rooms = parts.Skip(1).ToList();
                                OnRoomListReceived?.Invoke(rooms);
                                break;
                            }
                    
                    }
                });

                case PacketTypes.RoomForwardMessage:
                    // ROOM_FORWARD_MESSAGE|roomName|msgId|sender|content|originalSender
                    if (parts.Length >= 6)
                    {
                        OnChatMessage?.Invoke(new ChatMessageInfo
                        {
                            RoomName = parts[1],
                            MessageId = parts[2],
                            Sender = parts[3],
                            Content = parts[4],
                            IsForwarded = true,
                            OriginalSender = parts[5]
                        });
                    }
                    break;

                case PacketTypes.Error:
                    // ERROR|code|detail
                    if (parts.Length >= 3)
                        OnRoomError?.Invoke(parts[1], parts[2]);
                    break;
            }
        }

        private static OnlineUserInfo ParseUserListEntry(string entry)
        {
            // entry dạng "username:avatarBase64" - tách tại dấu ':' đầu tiên
            int idx = entry.IndexOf(':');
            if (idx < 0)
                return new OnlineUserInfo { Username = entry, AvatarBase64 = string.Empty };

            return new OnlineUserInfo
            {
                Username = entry.Substring(0, idx),
                AvatarBase64 = entry.Substring(idx + 1)
            };
        }

        private void HandleDisconnected(Socket socket)
        {
            if (_socket == socket)
                _socket = null;

            _isConnecting = false;
            Username = null;

            SafeClose(socket);

            OnStatusChanged?.Invoke("Mất kết nối với Server.");
            OnDisconnected?.Invoke();
        }

        private void SafeClose(Socket socket)
        {
            try { socket.Close(); }
            catch { }
        }

        private void ProcessReceiveBuffer(Action<string> handleLine)
        {
            while (true)
            {
                int newlineIndex = IndexOfNewline(_receiveBuffer);
                if (newlineIndex < 0)
                    break;

                string line = _receiveBuffer.ToString(0, newlineIndex);
                _receiveBuffer.Remove(0, newlineIndex + 1);

                if (line.EndsWith("\r"))
                    line = line.Substring(0, line.Length - 1);

                if (line.Length == 0)
                    continue;

                handleLine(line);
            }
        }
        public void Login(string username, string password)
        {
            string packet = $"{PacketTypes.Login}|{username}|{password}\n";
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            try
            {
                _socket?.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(
                    "Gửi login thất bại: " + ex.Message);
            }
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
        public void CreateRoom(string roomName, int maxUsers, bool isPrivate, string password)
        {
            if (!IsConnected)
                return;

            string packet;

            if (isPrivate)
            {
                packet =
                    $"CREATE_ROOM|{roomName}|{maxUsers}|PRIVATE|{password}\n";
            }
            else
            {
                packet =
                    $"CREATE_ROOM|{roomName}|{maxUsers}|PUBLIC\n";
            }

            _socket.Send(
                Encoding.UTF8.GetBytes(packet));
        }

        public void JoinRoom(string roomName, string password = "")
        {
            if (!IsConnected)
                return;
            string packet = $"JOIN_ROOM|{roomName}|{password}\n";
            _socket.Send(Encoding.UTF8.GetBytes(packet));
        }

        public void LeaveRoom(string roomName)
        {
            if (!IsConnected) return;
            string packet = $"LEAVE_ROOM|{roomName}\n";
            _socket.Send(Encoding.UTF8.GetBytes(packet));
        }
    }
}
