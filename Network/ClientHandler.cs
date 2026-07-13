using System;
using System.Net.Sockets;
using System.Text;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class ClientHandler
    {
        public Socket Socket { get; }
        public byte[] Buffer { get; } = new byte[1024];
        public StringBuilder ReceiveBuffer { get; } = new StringBuilder();

        public string Username { get; set; } = null;
        public bool IsLoggedIn => Username != null;
        public ClientHandler(Socket socket)
        {
            Socket = socket;
        }

        public void Send(byte[] data)
        {
            if (Socket.Connected)
                Socket.Send(data);
        }

        public void SendPacket(string plaintextPacket)
        {
            if (Socket.Connected)
            {
                string cleanPacket = plaintextPacket.TrimEnd('\r', '\n');
                string encryptedPacket = TCPIP_Collaborative_Chat_System.Services.EncryptionService.Encrypt(cleanPacket);
                string lineToSend = encryptedPacket + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(lineToSend);
                Socket.Send(buffer);
            }
        }
        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); } catch { }
            Socket.Close();
        }
        public string CurrentRoom { get; set; }
    }
}
