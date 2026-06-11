using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class MessageModel
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime SendAt { get; set; }
        public MessageType Type { get; set; }
        public MessageModel()
        {
            SendAt = DateTime.Now;
            Type = MessageType.Chat;
        }
        public MessageModel(string sender, string content, MessageType type = MessageType.Chat)
        {
            Sender = sender;
            Content = content;
            SendAt = DateTime.Now;
            Type = type;
        }
        public enum MessageType
        {
            Chat,
            System,
            Auth
        }
    }
}
