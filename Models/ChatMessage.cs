using System;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class ChatMessage
    {
        public Guid MessageId { get; set; }
        public string RoomName { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
        public bool IsReply { get; set; }
        public Guid? ReplyMessageId { get; set; }
        public bool IsForward { get; set; }
        public Guid? ForwardMessageId { get; set; }
    }
}
