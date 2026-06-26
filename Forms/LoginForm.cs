using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Database;
using TCPIP_Collaborative_Chat_System.Client;

namespace TCPIP_Collaborative_Chat_System.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AppMode _mode;
        public LoginForm(AppMode mode)
        {
            InitializeComponent();
            _mode = mode;
            AcceptButton = btnLogin;
            ConfigureLoginMode();
        }
        private void ConfigureLoginMode()
        {
            bool isClient = _mode == AppMode.Client;
            lblMode.Text = isClient ? "Client Login" : "Administrator Login";
            chkRemember.Visible = isClient;
            btnRegister.Visible = true;
            if (isClient)
            {
                LoadRememberUser();
                LoadSettings();
                LoadRemember();
            }
            else
            {
                txtUsername.Clear();
                txtPassword.Clear();
                txtUsername.Focus();
            }
        }
        private void LoadRemember()
        {
            if (!SettingsManager.Exists())
                return;
            bool remember = SettingsManager.Read("Remember") == "True";
            chkRemember.Checked = remember;
            if (!remember)
                return;
            txtUsername.Text = SettingsManager.Read("Username");

        }
        private void LoadSettings()
        {
            if (!SettingsManager.Exists())
                return;
                }

            chkRemember.Checked = SettingsManager.Read("Remember") == "True";

            txtUsername.Text = SettingsManager.Read("Username");
        }
        private void LoadRememberUser()
        {
            string username = UserRepository.GetRememberUser();

            if (!string.IsNullOrWhiteSpace(username))
            {
                txtUsername.Text = username;
                chkRemember.Checked = true;
                txtPassword.Focus();
            }
        }
        private void Login()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string hash = PasswordHasher.Hash(password);

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Vui lòng nhập Username", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập Password", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }
            // Kiểm tra tài khoản có tồn tại không
            if (!UserRepository.UserExists(username))
            {
                MessageBox.Show(
                    "Bạn chưa có tài khoản.\nVui lòng đăng ký.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // Kiểm tra mật khẩu
            if (!UserRepository.ValidateLogin(username, hash))
            {
                MessageBox.Show(
                    "Sai Username hoặc Password",
                    "Đăng nhập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }
            //Remember chỉ Client mới cần vì admin cần bảo mật cao
            if (_mode == AppMode.Client)
            {
                UserRepository.ClearRememberUsers();
                UserRepository.UpdateRememberMe(username, chkRemember.Checked);
            }

            // Client
            if (_mode == AppMode.Client)
            {
                TcpChatClientForm client = new TcpChatClientForm(username, password, chkRemember.Checked);
                client.Show();
                Hide();
                return;
            }

            // Server
            TcpChatServerForm server = new TcpChatServerForm();
            server.Show();
            Hide();
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm frm = new RegisterForm();

            if (frm.ShowDialog() == DialogResult.OK)
            {
                txtUsername.Text = frm.RegisteredUsername;
                txtPassword.Clear();
                txtPassword.Focus();
            }

            settings.Save();
        }
    }
}
