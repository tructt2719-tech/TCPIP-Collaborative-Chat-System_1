using System;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Database;

namespace TCPIP_Collaborative_Chat_System.Services
{
    public static class ReplyService
    {
        public static ChatMessage CreateReplyMessage(Guid newMsgId, Guid replyToId, string roomName, string sender, string content)
        {
            return new ChatMessage
            {
                MessageId = newMsgId,
                RoomName = roomName,
                Sender = sender,
                Content = content,
                Time = DateTime.Now,
                IsReply = true,
                ReplyMessageId = replyToId,
                IsForward = false,
                ForwardMessageId = null
            };
        }

        public static bool ValidateReply(Guid replyToId, out string error)
        {
            var origMsg = MessageRepository.GetMessageById(replyToId);
            if (origMsg == null)
            {
                error = "MESSAGE_NOT_FOUND";
                return false;
            }
            error = null;
            return true;
        }
    }
}
