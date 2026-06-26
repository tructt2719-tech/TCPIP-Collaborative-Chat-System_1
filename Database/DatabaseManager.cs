using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
namespace TCPIP_Collaborative_Chat_System.Database
{
    public static class DatabaseManager
    {
        private static string _dbFile =
            "chat.db";

        private static string _connectionString =
            $"Data Source={_dbFile};Version=3;";

        public static string ConnectionString
            => _connectionString;

        public static void Initialize()
        {
            if (!File.Exists(_dbFile))
            {
                SQLiteConnection.CreateFile(_dbFile);
            }
            CreateTables();
            CreateDefaultAdmin();
        }

        private static void CreateTables()
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string createUsers =
                @"CREATE TABLE IF NOT EXISTS Users
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'USER',
                    RememberMe INTEGER NOT NULL DEFAULT 0,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastLogin DATETIME
                );";

                string createRooms =
                @"CREATE TABLE IF NOT EXISTS Rooms
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RoomName TEXT UNIQUE,
                    Owner TEXT,
                    IsPrivate INTEGER,
                    Password TEXT,
                    MaxUsers INTEGER
                );";

                string createMessages =
                @"CREATE TABLE IF NOT EXISTS Messages
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RoomName TEXT,
                    Sender TEXT,
                    Content TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

                new SQLiteCommand(createUsers, conn)
                    .ExecuteNonQuery();

                new SQLiteCommand(createRooms, conn)
                    .ExecuteNonQuery();

                new SQLiteCommand(createMessages, conn)
                    .ExecuteNonQuery();
            }
        }
        private static void CreateDefaultAdmin()
        {
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM Users WHERE Username='admin';";
                SQLiteCommand check = new SQLiteCommand(sql, conn);
                long count = (long)check.ExecuteScalar();
                if (count == 0)
                {
                    string insert = @"INSERT INTO Users(Username, PasswordHash, Role) VALUES(@u, @p, @r);";
                    SQLiteCommand cmd = new SQLiteCommand(insert, conn);
                    cmd.Parameters.AddWithValue("@u", "admin");
                    // Tạm thời để plain text, bước sau sẽ chuyển sang SHA256
                    cmd.Parameters.AddWithValue("@p", PasswordHasher.Hash("admin123"));
                    cmd.Parameters.AddWithValue("@r", "ADMIN");
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}