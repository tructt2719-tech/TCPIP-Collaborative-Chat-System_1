using System.Collections.Generic;
using System.Data.SQLite;

namespace TCPIP_Collaborative_Chat_System.Database
{
    public static class FileRepository
    {
        public static void SaveFile(
            string roomName,
            string sender,
            string fileName,
            string filePath,
            long fileSize)
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();

                string sql =
                @"INSERT INTO Files
                (
                    RoomName,
                    Sender,
                    FileName,
                    FilePath,
                    FileSize
                )
                VALUES
                (
                    @room,
                    @sender,
                    @name,
                    @path,
                    @size
                );";

                SQLiteCommand cmd =
                    new SQLiteCommand(sql, conn);

                cmd.Parameters.AddWithValue("@room", roomName);
                cmd.Parameters.AddWithValue("@sender", sender);
                cmd.Parameters.AddWithValue("@name", fileName);
                cmd.Parameters.AddWithValue("@path", filePath);
                cmd.Parameters.AddWithValue("@size", fileSize);

                cmd.ExecuteNonQuery();
            }
        }

        public static string GetFilePath(string fileName)
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();

                string sql =
                @"SELECT FilePath
                  FROM Files
                  WHERE FileName=@name
                  ORDER BY Id DESC
                  LIMIT 1;";

                SQLiteCommand cmd =
                    new SQLiteCommand(sql, conn);

                cmd.Parameters.AddWithValue("@name", fileName);

                object result = cmd.ExecuteScalar();

                return result == null ? "" : result.ToString();
            }
        }

        public static List<string> GetFilesByRoom(string roomName)
        {
            List<string> files = new List<string>();

            using (SQLiteConnection conn =
                new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();

                string sql =
                @"SELECT FileName
                  FROM Files
                  WHERE RoomName=@room;";

                SQLiteCommand cmd =
                    new SQLiteCommand(sql, conn);

                cmd.Parameters.AddWithValue("@room", roomName);

                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    files.Add(reader["FileName"].ToString());
                }
            }

            return files;
        }
    }
}