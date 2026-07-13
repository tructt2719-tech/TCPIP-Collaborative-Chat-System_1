using System;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Database;

namespace TCPIP_Collaborative_Chat_System.Services
{
    public static class ForwardService
    {
        public static ChatMessage CreateForwardMessage(Guid newMsgId, Guid originalMsgId, string targetRoom, string sender, string content)
        {
            return new ChatMessage
            {
                MessageId = newMsgId,
                RoomName = targetRoom,
                Sender = sender,
                Content = content,
                Time = DateTime.Now,
                IsReply = false,
                ReplyMessageId = null,
                IsForward = true,
                ForwardMessageId = originalMsgId
            };
        }
    }
}
