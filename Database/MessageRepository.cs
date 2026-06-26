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
        public static bool SaveMessage(string roomName, string sender, string content)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO Messages(RoomName, Sender, Content) VALUES(@room, @sender, @content);";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@room", roomName);
                cmd.Parameters.AddWithValue("@sender", sender);
                cmd.Parameters.AddWithValue("@content", content);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public static List<MessageModel> GetMessages(string roomName)
        {
            List<MessageModel> list = new List<MessageModel>();
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT Id, RoomName, Sender, Content, CreatedAt FROM Messages WHERE RoomName=@room ORDER BY Id;";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@room", roomName);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MessageModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        RoomName = reader["RoomName"].ToString(),
                        Sender = reader["Sender"].ToString(),
                        Content = reader["Content"].ToString(),
                        CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString())
                    });
                }

                return list;
            }
        }
    }
}