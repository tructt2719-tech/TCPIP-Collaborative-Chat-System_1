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
        public event Action<string> OnStatusChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<string> OnUserListChanged;

        private static readonly UserStore SharedUserStore = new UserStore();

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

                ProcessReceiveBuffer(handler, line =>
                {
                    string[] parts = PacketParser.Parse(line);
                    if (parts.Length == 0) return;

                    string command = parts[0];

                    if (command == PacketTypes.Register)
                    {
                        HandleRegister(handler, parts);
                        return;
                    }

                    if (command == PacketTypes.Login)
                    {
                        HandleLogin(handler, parts);
                        return;
                    }

                    if (command == PacketTypes.Message && parts.Length >= 3)
                    {
                        if (!handler.IsLoggedIn)
                        {
                            SendTo(handler, PacketBuilder.BuildLoginFail("Bạn chưa đăng nhập"));
                            return;
                        }

                        handler.Status = "Online";
                        OnMessageReceived?.Invoke($"[STATUS] {handler.Username} -> {handler.Status}");

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

            if (!SharedUserStore.TryAuthenticate(username, password, out RegisteredUser _, out string authError))
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
            handler.Status = "LoggedIn";
            OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> LoggedIn");
            SendTo(handler, PacketBuilder.BuildLoginOk(username));
            handler.Status = "Online";
            OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> Online");

            string joinMsg = PacketBuilder.BuildSystem($"{username} đã tham gia");
            BroadcastToLoggedIn(joinMsg);

            string userListPacket = PacketBuilder.BuildUserList(GetOnlineUsernames());
            BroadcastToLoggedIn(userListPacket);

            OnStatusChanged?.Invoke($"{username} đã đăng nhập");
            OnUserListChanged?.Invoke(string.Join(", ", GetOnlineUsernames()));
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

            if (!string.IsNullOrEmpty(username))
            {
                OnMessageReceived?.Invoke($"[LIFECYCLE] {username} -> Disconnected");

                string leaveMsg = PacketBuilder.BuildSystem($"{username} đã thoát");
                BroadcastToLoggedIn(leaveMsg);

                string userListPacket = PacketBuilder.BuildUserList(GetOnlineUsernames());
                BroadcastToLoggedIn(userListPacket);

                OnUserListChanged?.Invoke(string.Join(", ", GetOnlineUsernames()));
            }
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