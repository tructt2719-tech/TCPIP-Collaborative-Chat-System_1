using System.Data.SQLite;

namespace TCPIP_Collaborative_Chat_System.Database
{
    public static class UserRepository
    {
        public static bool UserExists(string username)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Users WHERE Username=@u";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);
                long count = (long)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static bool AddUser(string username, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO Users(Username, PasswordHash) VALUES(@u, @p)";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public static bool ValidateLogin(string username, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM Users WHERE Username=@u AND PasswordHash=@p";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                long count = (long)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        public static void UpdateRememberMe(string username, bool remember)
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"UPDATE Users SET RememberMe=@remember WHERE Username=@user";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@remember", remember ? 1 : 0);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.ExecuteNonQuery();
            }
        }
        public static string GetRememberUser()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT Username FROM Users WHERE RememberMe=1 LIMIT 1";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                object result = cmd.ExecuteScalar();
                if (result == null)
                    return "";

                return result.ToString();
            }
        }
        public static void ClearRememberUsers()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("UPDATE Users SET RememberMe=0", conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}