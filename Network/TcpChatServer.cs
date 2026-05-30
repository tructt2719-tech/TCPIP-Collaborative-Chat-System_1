using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPIP_Collaborative_Chat_System.Shared;

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
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _serverSocket.Listen(10);
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

            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    try
                    {
                        client.Send(buffer);
                    }
                    catch
                    {
                        client.Close();
                        _clients.Remove(client);
                    }
                }
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

                string chunk = Encoding.UTF8.GetString(handler.Buffer, 0, size);
                handler.ReceiveBuffer.Append(chunk);

                ProcessReceiveBuffer(handler, line =>
                {
                    string[] parts = PacketParser.Parse(line);
                    if (parts.Length == 0) return;

                    string command = parts[0];

                    // LOGIN|Alice
                    if (command == PacketTypes.Login && parts.Length >= 2)
                    {
                        string username = parts[1].Trim();

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
                        SendTo(handler, PacketBuilder.BuildLoginOk(username));

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
            try { endpoint = handler.Socket.RemoteEndPoint?.ToString(); } catch { }

            handler.Close();
            lock (_clients)
                _clients.Remove(handler);

            OnClientDisconnected?.Invoke(endpoint);
            OnStatusChanged?.Invoke($"{_clients.Count} client(s) đang kết nối");
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

        // method gửi cho 1 client
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
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
