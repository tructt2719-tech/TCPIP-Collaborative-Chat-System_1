using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
<<<<<<< Updated upstream
=======
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Services.Security;
>>>>>>> Stashed changes
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class ChatClientManager
    {
        public string Username { get; private set; }
        public bool IsLoggedIn => Username != null;
        // Events để báo cho UI, class này không phụ thuộc trực tiếp vào WinForms
        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action OnDisconnected;
        public event Action<string> OnLoginResult;   
        public event Action<string> OnSystemMessage;
        public event Action<List<string>> OnUserListUpdated;

        private Socket _socket;
        private readonly byte[] _buffer = new byte[1024];
        private readonly StringBuilder _receiveBuffer = new StringBuilder();

        // Chặn trường hợp bấm Connect nhiều lần khi đang kết nối
        private bool _isConnecting;

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

                Socket socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

                // Gán socket hiện tại sau khi tạo thành công
                _socket = socket;

                // Truyền socket vào AsyncState để callback xử lý đúng socket,
                // tránh lỗi callback cũ dùng nhầm _socket mới.
                socket.BeginConnect(endpoint, HandleConnection, socket);
            }
            catch (Exception ex)
            {
                _isConnecting = false;
                _socket = null;
                OnStatusChanged?.Invoke("Kết nối thất bại: " + ex.Message);
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
            {
                return;
            }

            try
            {
                string packet = PacketBuilder.BuildMessage(senderName, message);
                byte[] buffer = Encoding.UTF8.GetBytes(packet);
                buffer = EncryptionService.Encrypt(buffer);

                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

<<<<<<< Updated upstream
=======
        public void SendRoomMessage(string roomName, string message)
        {
            if (!IsConnected)
                return;
            string packet = $"ROOM_MSG|{roomName}|{message}\n";
            byte[] data = Encoding.UTF8.GetBytes(packet);
            data = EncryptionService.Encrypt(data);
            _socket.Send(data);
        }

>>>>>>> Stashed changes
        public void Disconnect()
        {
            Socket socket = _socket;

            if (socket == null)
            {
                return;
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // Socket có thể đã bị server đóng trước, bỏ qua lỗi shutdown
            }

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

                // Nếu socket callback không còn là socket hiện tại thì bỏ qua
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
                {
                    _socket = null;
                }

                SafeClose(socket);
                OnStatusChanged?.Invoke("Kết nối thất bại: " + ex.Message);
            }
        }

        private void HandleDataReceived(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;

            if (socket == null)
            {
                return;
            }

            try
            {
                // Nếu callback này thuộc socket cũ thì bỏ qua
                if (_socket != socket)
                {
                    SafeClose(socket);
                    return;
                }

                int size = socket.EndReceive(ar);

                // size == 0 nghĩa là phía server đã đóng kết nối
                if (size == 0)
                {
                    HandleDisconnected(socket);
                    return;
                }

                byte[] encrypted = new byte[size];
                Array.Copy(_buffer, encrypted, size);
                byte[] decrypted;
                try
                {
                    decrypted = EncryptionService.Decrypt(encrypted);
                }
                catch
                {
                    OnStatusChanged?.Invoke( "Decrypt failed");
                    return;
                }
                string chunk = Encoding.UTF8.GetString(decrypted);
                _receiveBuffer.Append(chunk);
                ProcessReceiveBuffer(line =>
                {
                    string[] parts = PacketParser.Parse(line);
                    if (parts.Length == 0) return;
                    string command = parts[0];
                    switch (command)
                    {
                        case "LOGIN_OK":
                            Username = parts[1];  
                            OnLoginResult?.Invoke("OK:" + parts[1]);
                            break;

                        case "LOGIN_FAIL":
                            OnLoginResult?.Invoke("FAIL:" + (parts.Length > 1 ? parts[1] : ""));
                            break;

                        case "SYSTEM":
                            OnSystemMessage?.Invoke(parts.Length > 1 ? parts[1] : "");
                            break;

                        case "USER_LIST":
                            var users = parts.Skip(1).ToList();
                            OnUserListUpdated?.Invoke(users);
                            break;

                        case "MESSAGE":
                            if (parts.Length >= 3)
                                OnMessageReceived?.Invoke(parts[1] + ": " + parts[2]);
                            break;
                    }
                });

                // Tiếp tục nhận dữ liệu nếu socket hiện tại vẫn còn hợp lệ
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
                HandleDisconnected(socket);
            }
        }

        private void HandleDisconnected(Socket socket)
        {
            if (_socket == socket)
            {
                _socket = null;
            }

            _isConnecting = false;

            SafeClose(socket);

            OnStatusChanged?.Invoke("Mất kết nối với Server.");
            OnDisconnected?.Invoke();
        }

        private void SafeClose(Socket socket)
        {
            try
            {
                socket.Close();
            }
            catch
            {
                // Bỏ qua lỗi khi đóng socket
            }
        }

        private void ProcessReceiveBuffer(Action<string> handleLine)
        {
            while (true)
            {
                int newlineIndex = IndexOfNewline(_receiveBuffer);
                if (newlineIndex < 0)
                {
                    break;
                }

                string line = _receiveBuffer.ToString(0, newlineIndex);
                _receiveBuffer.Remove(0, newlineIndex + 1);

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
        public void Login(string username)
        {
<<<<<<< Updated upstream
            string packet = $"LOGIN|{username}\n";
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            try { _socket?.Send(buffer); }
            catch (Exception ex) { OnStatusChanged?.Invoke("Gửi login thất bại: " + ex.Message); }
=======
            string hash = PasswordHasher.Hash(password);
            string packet = $"{PacketTypes.Login}|{username}|{hash}\n";
            byte[] buffer = Encoding.UTF8.GetBytes(packet);
            buffer = EncryptionService.Encrypt(buffer);
            try
            {
                _socket?.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(
                    "Gửi login thất bại: " + ex.Message);
            }
>>>>>>> Stashed changes
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

            byte[] data = Encoding.UTF8.GetBytes(packet);
            data = EncryptionService.Encrypt(data);
            _socket.Send(data);
        }

        public void JoinRoom(string roomName, string password = "")
        {
            if (!IsConnected)
                return;
            string packet = $"JOIN_ROOM|{roomName}|{password}\n";
            byte[] data = Encoding.UTF8.GetBytes(packet);
            data = EncryptionService.Encrypt(data);
            _socket.Send(data);
        }

        public void LeaveRoom(string roomName)
        {
            if (!IsConnected) return;
            string packet = $"LEAVE_ROOM|{roomName}\n";
            byte[] data = Encoding.UTF8.GetBytes(packet);
            data = EncryptionService.Encrypt(data);
            _socket.Send(data);
        }
        public void DeleteRoom(string roomName)
        {
            if (!IsConnected) return;
            string packet = $"DELETE_ROOM|{roomName}\n";
            byte[] data = Encoding.UTF8.GetBytes(packet);
            data = EncryptionService.Encrypt(data);
            _socket.Send(data);
        }
>>>>>>> Stashed changes
    }
}