using System;
using System.Collections.Concurrent;

namespace TCPIP_Collaborative_Chat_System.Network
{
    public class RegisteredUser
    {
        public string Username { get; }
        public string Password { get; }
        public string Email { get; }
        public string AvatarBase64 { get; }

        public RegisteredUser(
            string username,
            string password,
            string email,
            string avatarBase64)
        {
            Username = username;
            Password = password;
            Email = email;
            AvatarBase64 = avatarBase64 ?? string.Empty;
        }
    }

    public class UserStore
    {
        private readonly ConcurrentDictionary<string, RegisteredUser> _users =
            new ConcurrentDictionary<string, RegisteredUser>(
                StringComparer.OrdinalIgnoreCase);

        public bool TryRegister(
            string username,
            string password,
            string email,
            string avatarBase64,
            out string error)
        {
            error = null;

            if (!ValidateUsername(username, out error))
                return false;

            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Password không được rỗng";
                return false;
            }

            if (password.Length < 4)
            {
                error = "Password phải có ít nhất 4 ký tự";
                return false;
            }

            if (!ValidateEmail(email, out error))
                return false;

            var user = new RegisteredUser(
                username.Trim(),
                password,
                email.Trim(),
                avatarBase64 ?? string.Empty);

            if (!_users.TryAdd(user.Username, user))
            {
                error = "Username đã tồn tại";
                return false;
            }

            return true;
        }

        public bool TryAuthenticate(
            string username,
            string password,
            out RegisteredUser user,
            out string error)
        {
            user = null;
            error = null;

            if (!ValidateUsername(username, out error))
                return false;

            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Password không được rỗng";
                return false;
            }

            if (!_users.TryGetValue(username.Trim(), out user))
            {
                error = "Tài khoản chưa tồn tại";
                return false;
            }

            if (!string.Equals(user.Password, password, StringComparison.Ordinal))
            {
                user = null;
                error = "Mật khẩu không đúng";
                return false;
            }

            return true;
        }

        public bool Exists(string username)
        {
            return !string.IsNullOrWhiteSpace(username)
                && _users.ContainsKey(username.Trim());
        }

        public static bool ValidateUsername(string username, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                error = "Username không được rỗng";
                return false;
            }

            username = username.Trim();

            if (username.Length < 1 || username.Length > 20)
            {
                error = "Username phải từ 1-20 ký tự";
                return false;
            }

            if (username.Contains("|") || username.Contains("\n") || username.Contains("\r"))
            {
                error = "Username chứa ký tự không hợp lệ";
                return false;
            }

            return true;
        }

        private static bool ValidateEmail(string email, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(email))
            {
                error = "Email không được rỗng";
                return false;
            }

            email = email.Trim();

            if (email.Contains("|") || email.Contains("\n") || email.Contains("\r"))
            {
                error = "Email chứa ký tự không hợp lệ";
                return false;
            }

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1 || !email.Contains("."))
            {
                error = "Email không hợp lệ";
                return false;
            }

            return true;
        }
    }
}