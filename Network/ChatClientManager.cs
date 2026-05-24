using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class ChatClientManager
    {
        // Events để báo cho UI, class này không phụ thuộc trực tiếp vào WinForms
        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action OnDisconnected;

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

                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi tin nhắn thất bại: " + ex.Message);
                HandleDisconnected(socket);
            }
        }

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

                string chunk = Encoding.UTF8.GetString(_buffer, 0, size);
                _receiveBuffer.Append(chunk);

                ProcessReceiveBuffer(line =>
                {
                    string[] parts = PacketParser.Parse(line);

                    if (parts.Length >= 3 && parts[0] == PacketTypes.Message)
                    {
                        string senderName = parts[1];
                        string message = parts[2];

                        OnMessageReceived?.Invoke(senderName + ": " + message);
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
    }
}