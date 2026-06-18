using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class ChatClientManager
    {
        public string Username { get; private set; }
        public bool IsLoggedIn => Username != null;

        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action OnDisconnected;
        public event Action<string> OnLoginResult;
        public event Action<string> OnSystemMessage;
        public event Action<List<string>> OnUserListUpdated;

        private Socket _socket;
        private readonly byte[] _buffer = new byte[8192];

        public Socket _socket;
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

                    Console.WriteLine($"[TryLogin] Attempt {attempt}: sent LOGIN for '{username}'");

                    socket.BeginReceive(
                        _buffer,
                        0,
                        _buffer.Length,
                        SocketFlags.None,
                        HandleDataReceived,
                        socket);

                    if (!_loginWaitHandle.Wait(timeoutMs))
                    {
                        Console.WriteLine($"[TryLogin] Attempt {attempt}: timeout after {timeoutMs}ms");

                        if (attempt < maxAttempts)
                        {
                            Console.WriteLine($"[TryLogin] Attempt {attempt} timed out, retrying...");
                            continue;
                        }

                        error = $"Hết thời gian chờ phản hồi từ server ({timeoutMs}ms). " +
                                "Kiểm tra server đang chạy và port đúng.";
                        return false;
                    }

                    error = _loginSuccess ? null : _loginMessage;
                    Console.WriteLine(_loginSuccess
                        ? $"[TryLogin] Success for '{username}'"
                        : $"[TryLogin] Failed: {_loginMessage}");
                    return _loginSuccess;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TryLogin] Attempt {attempt} exception: {ex.Message}");

                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"[TryLogin] Attempt {attempt} error, retrying...");
                        continue;
                    }

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

        public void Send(string senderName, string message)
        {
            Socket socket = _socket;

            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                string packet = PacketBuilder.BuildMessage(senderName, message);
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }


        public void Login(string username, string password);

        public void SendReply(string senderName, string message, string replyToId, string replyToSender)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            try
            {
                string packet = $"RELY_MESSAGE|{senderName}|{message}|{replyToId}|{replyToSender}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn trả lời thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public void SendForward(string senderName, string message, string originalSender)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            try
            {
                string packet = $"FORWARD_MESSAGE|{senderName}|{originalSender}|{message}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn chuyển tiếp thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public void SendEmojiReaction(string senderName, string messageId, string emoji)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }
            try
            {
                string packet = $"EMOJI_REACTION|{senderName}|{messageId}|{emoji}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi phản ứng emoji thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

        public void Disconnect()

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
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi login thất bại: " + ex.Message);
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
                        Console.WriteLine("[TryLogin] Server closed connection before response");
                        _loginWaitHandle.Set();
                    }
                    else
                    {
                        HandleDisconnected(socket);
                    }
                    return;
                }

                string chunk = Encoding.UTF8.GetString(_buffer, 0, size);

                if (_waitingForLogin)
                    Console.WriteLine($"[TryLogin] Received chunk ({size} bytes): {chunk}");

                _receiveBuffer.Append(chunk);

                ProcessReceiveBuffer(line =>
                {
                    if (_waitingForLogin)
                        Console.WriteLine($"[TryLogin] Parsed line: {line}");

                    string[] parts = PacketParser.Parse(line);
                    if (parts.Length == 0) return;

                    string command = parts[0].Trim();

                    if (_waitingForLogin)
                        Console.WriteLine($"[TryLogin] Command: {command}");

                    switch (command)
                    {
                        case PacketTypes.LoginOk:
                            if (_waitingForLogin)
                            {
                                _loginSuccess = true;
                                _loginMessage = parts.Length > 1 ? parts[1] : "OK";
                                Console.WriteLine($"[TryLogin] LOGIN_OK received for '{_loginMessage}'");
                                _loginWaitHandle.Set();
                            }
                            else
                            {
                                Username = parts.Length > 1 ? parts[1] : null;
                                OnLoginResult?.Invoke("OK:" + (parts.Length > 1 ? parts[1] : ""));
                            }
                            break;

                        case PacketTypes.LoginFail:
                            if (_waitingForLogin)
                            {
                                _loginSuccess = false;
                                _loginMessage = parts.Length > 1 ? parts[1] : "Đăng nhập thất bại";
                                Console.WriteLine($"[TryLogin] LOGIN_FAIL received: {_loginMessage}");
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
                            var users = parts.Skip(1).ToList();
                            OnUserListUpdated?.Invoke(users);
                            break;

                        case PacketTypes.Message:
                            if (parts.Length >= 3)
                                OnMessageReceived?.Invoke(parts[1] + ": " + parts[2]);
                            break;
                        case "RELY_MESSAGE":
                            if (parts.Length >= 5)
                            {
                                string sender = parts[1];
                                string content = parts[2];
                                string replyToSender = parts[3];
                                OnMessageReceived?.Invoke($"{sender} (trả lời {replyToSender}): {content}");
                            }
                            break;
                        case "FORWARD_MESSAGE":
                            if (parts.Length >= 4)
                            {
                                string fowardBy = parts[1];
                                string originalSender = parts[2];
                                string content = parts[3];
                                OnMessageReceived?.Invoke($"{fowardBy} (chuyển tiếp từ {originalSender}): {content}");
                            }
                            break;
                        case "EMOJI_REACTION_BROADCAST":
                            if (parts.Length >= 4)
                            {
                                string reactor = parts[1];
                                string messageId = parts[2];
                                string emoji = parts[3];
                                OnMessageReceived?.Invoke($"{reactor} đã phản ứng với tin nhắn {messageId} bằng {emoji}");
                            }
                            break;
                    }
                });

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
                    Console.WriteLine("[TryLogin] Receive exception, signaling wait handle");
                    _loginWaitHandle.Set();
                }
                else
                {
                    HandleDisconnected(socket);
                }
            }
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