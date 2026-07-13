using System;
using System.Collections.Generic;
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
            UpgradeDatabase();
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
                    Avatar TEXT,
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
                    MessageId TEXT UNIQUE,
                    RoomName TEXT,
                    Sender TEXT,
                    Content TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsReply INTEGER DEFAULT 0,
                    ReplyMessageId TEXT,
                    IsForward INTEGER DEFAULT 0,
                    ForwardMessageId TEXT
                );";
                string createFiles =
                @"CREATE TABLE IF NOT EXISTS Files
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RoomName TEXT,
                    Sender TEXT,
                    FileName TEXT,
                    FilePath TEXT,
                    FileSize INTEGER,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

                new SQLiteCommand(createUsers, conn)
                    .ExecuteNonQuery();

                new SQLiteCommand(createRooms, conn)
                    .ExecuteNonQuery();
                new SQLiteCommand(createFiles, conn)
                    .ExecuteNonQuery();
                new SQLiteCommand(createMessages, conn)
                    .ExecuteNonQuery();
            }
        }
        private static void UpgradeDatabase()
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(_connectionString))
            {
                conn.Open();

                SQLiteCommand cmd =
                    new SQLiteCommand(
                        "PRAGMA table_info(Users);",
                        conn);

                SQLiteDataReader reader =
                    cmd.ExecuteReader();

                bool hasAvatar = false;

                while (reader.Read())
                {
                    if (reader["name"].ToString() == "Avatar")
                    {
                        hasAvatar = true;
                        break;
                    }
                }

                reader.Close();

                if (!hasAvatar)
                {
                    SQLiteCommand alter =
                        new SQLiteCommand(
                            "ALTER TABLE Users ADD COLUMN Avatar TEXT;",
                            conn);

                    alter.ExecuteNonQuery();
                }

                // Upgrade Messages table
                SQLiteCommand cmdMsg = new SQLiteCommand("PRAGMA table_info(Messages);", conn);
                using (SQLiteDataReader readerMsg = cmdMsg.ExecuteReader())
                {
                    List<string> columns = new List<string>();
                    while (readerMsg.Read())
                    {
                        columns.Add(readerMsg["name"].ToString());
                    }
                    if (!columns.Contains("MessageId"))
                    {
                        using (var alterCmd = new SQLiteCommand("ALTER TABLE Messages ADD COLUMN MessageId TEXT UNIQUE DEFAULT NULL;", conn))
                        {
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                    if (!columns.Contains("IsForward"))
                    {
                        using (var alterCmd = new SQLiteCommand("ALTER TABLE Messages ADD COLUMN IsForward INTEGER DEFAULT 0;", conn))
                        {
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                    if (!columns.Contains("IsReply"))
                    {
                        using (var alterCmd = new SQLiteCommand("ALTER TABLE Messages ADD COLUMN IsReply INTEGER DEFAULT 0;", conn))
                        {
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                    if (!columns.Contains("ReplyMessageId"))
                    {
                        using (var alterCmd = new SQLiteCommand("ALTER TABLE Messages ADD COLUMN ReplyMessageId TEXT DEFAULT NULL;", conn))
                        {
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                    if (!columns.Contains("ForwardMessageId"))
                    {
                        using (var alterCmd = new SQLiteCommand("ALTER TABLE Messages ADD COLUMN ForwardMessageId TEXT DEFAULT NULL;", conn))
                        {
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                }
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