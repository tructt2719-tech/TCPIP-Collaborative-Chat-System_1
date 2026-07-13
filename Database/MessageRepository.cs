using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using TCPIP_Collaborative_Chat_System.Models;

namespace TCPIP_Collaborative_Chat_System.Database
{
    public static class MessageRepository
    {
        public static bool SaveMessage(ChatMessage msg)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO Messages(MessageId, RoomName, Sender, Content, CreatedAt, IsReply, ReplyMessageId, IsForward, ForwardMessageId) 
                               VALUES(@msgId, @room, @sender, @content, @createdAt, @isReply, @replyMessageId, @isForward, @forwardMessageId);";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@msgId", msg.MessageId.ToString());
                cmd.Parameters.AddWithValue("@room", (object)msg.RoomName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sender", msg.Sender);
                cmd.Parameters.AddWithValue("@content", msg.Content);
                cmd.Parameters.AddWithValue("@createdAt", msg.Time);
                cmd.Parameters.AddWithValue("@isReply", msg.IsReply ? 1 : 0);
                cmd.Parameters.AddWithValue("@replyMessageId", msg.ReplyMessageId.HasValue ? msg.ReplyMessageId.Value.ToString() : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@isForward", msg.IsForward ? 1 : 0);
                cmd.Parameters.AddWithValue("@forwardMessageId", msg.ForwardMessageId.HasValue ? msg.ForwardMessageId.Value.ToString() : (object)DBNull.Value);
                
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static List<ChatMessage> GetMessages(string roomName)
        {
            List<ChatMessage> list = new List<ChatMessage>();
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT MessageId, RoomName, Sender, Content, CreatedAt, IsReply, ReplyMessageId, IsForward, ForwardMessageId FROM Messages WHERE RoomName=@room ORDER BY Id;";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@room", roomName);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string msgIdStr = reader["MessageId"] != DBNull.Value ? reader["MessageId"].ToString() : null;
                        Guid msgId = string.IsNullOrEmpty(msgIdStr) ? Guid.NewGuid() : Guid.Parse(msgIdStr);
                        
                        string replyIdStr = reader["ReplyMessageId"] != DBNull.Value ? reader["ReplyMessageId"].ToString() : null;
                        Guid? replyId = string.IsNullOrEmpty(replyIdStr) ? (Guid?)null : Guid.Parse(replyIdStr);

                        string forwardIdStr = reader["ForwardMessageId"] != DBNull.Value ? reader["ForwardMessageId"].ToString() : null;
                        Guid? forwardId = string.IsNullOrEmpty(forwardIdStr) ? (Guid?)null : Guid.Parse(forwardIdStr);

                        list.Add(new ChatMessage
                        {
                            MessageId = msgId,
                            RoomName = reader["RoomName"] != DBNull.Value ? reader["RoomName"].ToString() : null,
                            Sender = reader["Sender"].ToString(),
                            Content = reader["Content"].ToString(),
                            Time = DateTime.Parse(reader["CreatedAt"].ToString()),
                            IsReply = reader["IsReply"] != DBNull.Value && Convert.ToInt32(reader["IsReply"]) == 1,
                            ReplyMessageId = replyId,
                            IsForward = reader["IsForward"] != DBNull.Value && Convert.ToInt32(reader["IsForward"]) == 1,
                            ForwardMessageId = forwardId
                        });
                    }
                }
            }
            return list;
        }

        public static ChatMessage GetMessageById(Guid messageId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT MessageId, RoomName, Sender, Content, CreatedAt, IsReply, ReplyMessageId, IsForward, ForwardMessageId FROM Messages WHERE MessageId=@id;";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", messageId.ToString());
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string msgIdStr = reader["MessageId"] != DBNull.Value ? reader["MessageId"].ToString() : null;
                        Guid msgId = string.IsNullOrEmpty(msgIdStr) ? Guid.NewGuid() : Guid.Parse(msgIdStr);
                        
                        string replyIdStr = reader["ReplyMessageId"] != DBNull.Value ? reader["ReplyMessageId"].ToString() : null;
                        Guid? replyId = string.IsNullOrEmpty(replyIdStr) ? (Guid?)null : Guid.Parse(replyIdStr);

                        string forwardIdStr = reader["ForwardMessageId"] != DBNull.Value ? reader["ForwardMessageId"].ToString() : null;
                        Guid? forwardId = string.IsNullOrEmpty(forwardIdStr) ? (Guid?)null : Guid.Parse(forwardIdStr);

                        return new ChatMessage
                        {
                            MessageId = msgId,
                            RoomName = reader["RoomName"] != DBNull.Value ? reader["RoomName"].ToString() : null,
                            Sender = reader["Sender"].ToString(),
                            Content = reader["Content"].ToString(),
                            Time = DateTime.Parse(reader["CreatedAt"].ToString()),
                            IsReply = reader["IsReply"] != DBNull.Value && Convert.ToInt32(reader["IsReply"]) == 1,
                            ReplyMessageId = replyId,
                            IsForward = reader["IsForward"] != DBNull.Value && Convert.ToInt32(reader["IsForward"]) == 1,
                            ForwardMessageId = forwardId
                        };
                    }
                }
            }
            return null;
        }

        public static bool DeleteMessage(Guid messageId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"DELETE FROM Messages WHERE MessageId=@id;";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", messageId.ToString());
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}