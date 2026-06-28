using System.Collections.Generic;
using System.Data.SQLite;
using TCPIP_Collaborative_Chat_System.Models;

namespace TCPIP_Collaborative_Chat_System.Database
{
    public class RoomRepository
    {
        public void AddRoom(ChatRoom room)
        {
            using (var conn =
                new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();

                string sql = @"INSERT INTO Rooms(RoomName, Owner, IsPrivate, Password, MaxUsers) VALUES(@RoomName, @Owner, @IsPrivate, @Password, @MaxUsers)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomName", room.RoomName);

                    cmd.Parameters.AddWithValue("@Owner", room.Owner);

                    cmd.Parameters.AddWithValue("@IsPrivate", room.IsPrivate ? 1 : 0);

                    cmd.Parameters.AddWithValue("@Password", room.Password);

                    cmd.Parameters.AddWithValue("@MaxUsers", room.MaxUsers);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ChatRoom> GetAllRooms()
        {
            List<ChatRoom> rooms = new List<ChatRoom>();
            using (var conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Rooms";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new ChatRoom
                            {
                                RoomName = reader["RoomName"].ToString(),

                                Owner = reader["Owner"].ToString(),

                                MaxUsers = int.Parse(reader["MaxUsers"].ToString()),

                                IsPrivate = reader["IsPrivate"].ToString() == "1",

                                Password = reader["Password"].ToString()
                            });
                    }
                }
            }

            return rooms;
        }

        public bool RoomExists(string roomName)
        {
            using (var conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Rooms WHERE RoomName=@RoomName";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomName", roomName);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public void DeleteNonSystemRooms()
        {
            using (var conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "DELETE FROM Rooms WHERE Owner != 'SYSTEM'", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}