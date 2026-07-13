using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Shared;
using System.IO;
using TCPIP_Collaborative_Chat_System.Models;

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
        public event Action<ChatMessage> OnRoomMsgReceived;
        public event Action<ChatMessage> OnRoomHistoryReceived;
        public event Action<ChatMessage> OnReplyMsgReceived;
        public event Action<ChatMessage> OnForwardMsgReceived;
        public event Action<ChatMessage> OnForwardPrivateReceived;
        public event Action<Guid, string> OnDeleteMsgReceived;
        public event Action<string> OnRoomUsers;
        public event Action<string> OnRoomUserJoined;
        public event Action<string> OnRoomUserLeft;
        public event Action<List<string>> OnRoomListReceived;
        public event Action<string> OnDeleteRoomResult;
        public event Action<string> OnRoomDeleted;
        public event Action<string, string, string, long> OnFileReceived;
        public event Action<string, byte[]> OnFileDataReceived;

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
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            string packet = PacketBuilder.BuildMessage(senderName, message);
            SendPacket(packet);
        }

        public void SendRoomMessage(string roomName, string message)
        {
            string packet = $"ROOM_MSG|{roomName}|{message}\n";
            SendPacket(packet);
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
                    string decryptedLine;
                    try
                    {
                        decryptedLine = TCPIP_Collaborative_Chat_System.Services.EncryptionService.Decrypt(line);
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged?.Invoke($"[SECURITY ALERT] Decrypt failed: {ex.Message}");
                        return;
                    }

                    string[] parts = PacketParser.Parse(decryptedLine);
                    if (parts.Length == 0) return;

                    string command = parts[0];

                    switch (command)
                    {
                        case "LOGIN_OK":
                            Username = parts[1];  // THÊM DÒNG NÀY
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

                        case "ROOM_MSG":
                            if (parts.Length >= 5)
                            {
                                Guid msgId;
                                if (Guid.TryParse(parts[1], out msgId))
                                {
                                    string rName = parts[2];
                                    string snd = parts[3];
                                    string cnt = parts[4];
                                    var msg = new ChatMessage
                                    {
                                        MessageId = msgId,
                                        RoomName = rName,
                                        Sender = snd,
                                        Content = cnt,
                                        Time = DateTime.Now,
                                        IsReply = false,
                                        ReplyMessageId = null,
                                        IsForward = false,
                                        ForwardMessageId = null
                                    };
                                    OnRoomMsgReceived?.Invoke(msg);
                                }
                            }
                            break;

                        case "ROOM_HISTORY":
                            {
                                if (parts.Length >= 6)
                                {
                                    string rName = parts[1];
                                    string snd = parts[2];
                                    string cnt = parts[3];
                                    DateTime time;
                                    if (DateTime.TryParse(parts[4], out time))
                                    {
                                        Guid msgId;
                                        if (Guid.TryParse(parts[5], out msgId))
                                        {
                                            bool isReply = parts.Length > 6 && parts[6] == "1";
                                            Guid? replyId = null;
                                            Guid tempReplyId;
                                            if (parts.Length > 7 && Guid.TryParse(parts[7], out tempReplyId))
                                                replyId = tempReplyId;

                                            bool isForward = parts.Length > 8 && parts[8] == "1";
                                            Guid? fwdId = null;
                                            Guid tempFwdId;
                                            if (parts.Length > 9 && Guid.TryParse(parts[9], out tempFwdId))
                                                fwdId = tempFwdId;

                                            var msg = new ChatMessage
                                            {
                                                MessageId = msgId,
                                                RoomName = rName,
                                                Sender = snd,
                                                Content = cnt,
                                                Time = time,
                                                IsReply = isReply,
                                                ReplyMessageId = replyId,
                                                IsForward = isForward,
                                                ForwardMessageId = fwdId
                                            };
                                            OnRoomHistoryReceived?.Invoke(msg);
                                        }
                                    }
                                }
                                break;
                            }

                        case "REPLY_MSG":
                            if (parts.Length >= 6)
                            {
                                Guid msgId = Guid.Parse(parts[1]);
                                Guid replyId = Guid.Parse(parts[2]);
                                string rName = parts[3];
                                string snd = parts[4];
                                string cnt = parts[5];

                                var msg = new ChatMessage
                                {
                                    MessageId = msgId,
                                    RoomName = rName,
                                    Sender = snd,
                                    Content = cnt,
                                    Time = DateTime.Now,
                                    IsReply = true,
                                    ReplyMessageId = replyId,
                                    IsForward = false,
                                    ForwardMessageId = null
                                };
                                OnReplyMsgReceived?.Invoke(msg);
                            }
                            break;

                        case "FORWARD_MSG":
                            if (parts.Length >= 6)
                            {
                                Guid msgId = Guid.Parse(parts[1]);
                                Guid origId = Guid.Parse(parts[2]);
                                string rName = parts[3];
                                string snd = parts[4];
                                string cnt = parts[5];

                                var msg = new ChatMessage
                                {
                                    MessageId = msgId,
                                    RoomName = rName,
                                    Sender = snd,
                                    Content = cnt,
                                    Time = DateTime.Now,
                                    IsReply = false,
                                    ReplyMessageId = null,
                                    IsForward = true,
                                    ForwardMessageId = origId
                                };
                                OnForwardMsgReceived?.Invoke(msg);
                            }
                            break;

                        case "FORWARD_PRIVATE":
                            if (parts.Length >= 6)
                            {
                                Guid msgId = Guid.Parse(parts[1]);
                                Guid origId = Guid.Parse(parts[2]);
                                string targetUser = parts[3];
                                string snd = parts[4];
                                string cnt = parts[5];

                                var msg = new ChatMessage
                                {
                                    MessageId = msgId,
                                    RoomName = targetUser,
                                    Sender = snd,
                                    Content = cnt,
                                    Time = DateTime.Now,
                                    IsReply = false,
                                    ReplyMessageId = null,
                                    IsForward = true,
                                    ForwardMessageId = origId
                                };
                                OnForwardPrivateReceived?.Invoke(msg);
                            }
                            break;

                        case "DELETE_MSG":
                            if (parts.Length >= 3)
                            {
                                Guid msgId = Guid.Parse(parts[1]);
                                string rName = parts[2];
                                OnDeleteMsgReceived?.Invoke(msgId, rName);
                            }
                            break;

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
                        case "DELETE_ROOM_OK":
                            OnDeleteRoomResult?.Invoke(
                                "OK:" + (parts.Length > 1 ? parts[1] : ""));
                            break;
                        case "DELETE_ROOM_FAIL":
                            OnDeleteRoomResult?.Invoke(
                                "FAIL:" + (parts.Length > 1 ? parts[1] : ""));
                            break;
                        case "ROOM_DELETED":
                            if (parts.Length >= 2)
                                OnRoomDeleted?.Invoke(parts[1]);
                            break;
                        case PacketTypes.FileInfo:
                            {
                                string room = parts[1];
                                string sender = parts[2];
                                string fileName = parts[3];

                                long fileSize = 0;
                                long.TryParse(parts[4], out fileSize);

                                OnMessageReceived?.Invoke(
                                    $"{sender} đã gửi file: {fileName}");

                                OnFileReceived?.Invoke(
                                    room,
                                    sender,
                                    fileName,
                                    fileSize);

                                break;
                            }
                        case PacketTypes.FileData:
                            {
                                if (parts.Length < 3)
                                    break;

                                string fileName = parts[1];

                                string base64 = parts[2];

                                byte[] data =
                                    Convert.FromBase64String(base64);

                                OnFileDataReceived?.Invoke(
                                    fileName,
                                    data);

                                break;
                            }
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
        public void Login(string username, string password)
        {
            string packet = $"{PacketTypes.Login}|{username}|{password}\n";
            SendPacket(packet);
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
        public void CreateRoom(string roomName, int maxUsers, bool isPrivate, string password)
        {
            string packet = isPrivate
                ? $"CREATE_ROOM|{roomName}|{maxUsers}|PRIVATE|{password}\n"
                : $"CREATE_ROOM|{roomName}|{maxUsers}|PUBLIC\n";
            SendPacket(packet);
        }

        public void JoinRoom(string roomName, string password = "")
        {
            string packet = $"JOIN_ROOM|{roomName}|{password}\n";
            SendPacket(packet);
        }

        public void LeaveRoom(string roomName)
        {
            string packet = $"LEAVE_ROOM|{roomName}\n";
            SendPacket(packet);
        }
        public void DeleteRoom(string roomName)
        {
            string packet = $"DELETE_ROOM|{roomName}\n";
            SendPacket(packet);
        }
        public void SendFile(string filePath, string roomName = "")
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                // Header includes roomName so server can notify the room
                string header = string.IsNullOrWhiteSpace(roomName)
                    ? $"{PacketTypes.FileBegin}|{fileName}|{fileData.Length}\n"
                    : $"{PacketTypes.FileBegin}|{fileName}|{fileData.Length}|{roomName}\n";
                SendPacket(header);

                string base64 = Convert.ToBase64String(fileData);
                string chunk = $"{PacketTypes.FileChunk}|{base64}\n";
                SendPacket(chunk);

                string end = $"{PacketTypes.FileEnd}\n";
                SendPacket(end);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Lỗi gửi file: " + ex.Message);
            }
        }
        public void DownloadFile(string fileName)
        {
            string packet = PacketBuilder.BuildFileDownload(fileName);
            SendPacket(packet);
        }

        public void SendReply(string roomName, Guid replyToMessageId, string content)
        {
            Guid newMessageId = Guid.NewGuid();
            string packet = PacketBuilder.BuildReplyMsg(newMessageId, replyToMessageId, roomName, Username, content);
            SendPacket(packet);
        }

        public void SendForwardRoom(Guid originalMsgId, string targetRoom)
        {
            Guid newMessageId = Guid.NewGuid();
            string packet = PacketBuilder.BuildForwardMsg(newMessageId, originalMsgId, targetRoom, Username);
            SendPacket(packet);
        }

        public void SendForwardPrivate(Guid originalMsgId, string targetUser)
        {
            Guid newMessageId = Guid.NewGuid();
            string packet = PacketBuilder.BuildForwardPrivate(newMessageId, originalMsgId, targetUser, Username);
            SendPacket(packet);
        }

        public void SendDeleteMsg(Guid messageId, string roomName)
        {
            string packet = PacketBuilder.BuildDeleteMsg(messageId, roomName);
            SendPacket(packet);
        }

        private void SendPacket(string plaintextPacket)
        {
            Socket socket = _socket;
            if (socket == null || !socket.Connected)
            {
                OnStatusChanged?.Invoke("Chưa kết nối đến Server.");
                return;
            }

            try
            {
                string cleanPacket = plaintextPacket.TrimEnd('\r', '\n');
                string encryptedPacket = TCPIP_Collaborative_Chat_System.Services.EncryptionService.Encrypt(cleanPacket);
                string lineToSend = encryptedPacket + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(lineToSend);
                socket.Send(buffer);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Gửi packet thất bại: " + ex.Message);
            }
        }
    }
}