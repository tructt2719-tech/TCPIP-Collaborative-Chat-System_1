using System;
using System.Net.Sockets;
using System.Text;
using TCPIP_Collaborative_Chat_System.Services.Security;

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
            if (!Socket.Connected)
                return;
            byte[] encrypted = EncryptionService.Encrypt(data);
            Socket.Send(encrypted);
        }

        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); } catch { }
            Socket.Close();
        }
    }
}
