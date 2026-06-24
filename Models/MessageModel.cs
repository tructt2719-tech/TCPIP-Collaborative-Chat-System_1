using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class MessageModel
    {
        public string MessageId { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime SendAt { get; set; }
        public MessageType Type { get; set; }
        public string Room { get; set; }
        public bool IsReply { get; set; }
        public string ReplyToId { get; set; }
        public string ReplyToSender { get; set; }
        public string ReplyToPreview { get; set; }
        public bool IsForwarded { get; set; }
        public string OriginalSender { get; set; }

        public MessageModel()
        {
            MessageId = Guid.NewGuid().ToString("N");
            SendAt = DateTime.Now;
            Type = MessageType.Chat;
        }

        public MessageModel(string sender, string content, MessageType type = MessageType.Chat)
        {
            MessageId = Guid.NewGuid().ToString("N");
            Sender = sender;
            Content = content;
            SendAt = DateTime.Now;
            Type = type;
        }

        public static string MakePreview(string content, int maxLength = 55)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            string clean = content.Replace("|", " ").Trim();
            return clean.Length <= maxLength ? clean : clean.Substring(0, maxLength) + "...";
        }

        public enum MessageType
        {
            Chat,
            System,
        }
    }
}
