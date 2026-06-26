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
            LoadRememberUser();
            LoadSettings();
        }
        private void LoadSettings()
        {
            if (!SettingsManager.Exists())
                return;

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
            
            // Remember 
            UserRepository.ClearRememberUsers();
            UserRepository.UpdateRememberMe(username, chkRemember.Checked);
            if (chkRemember.Checked)
            {
                SettingsManager.Save(username, true, "127.0.0.1", 12345);
            }
            else
            {
                SettingsManager.Delete();
            }

            //Client
            if (_mode == AppMode.Client)
            {
                TcpChatClientForm frm = new TcpChatClientForm(username, password);
                frm.Show();
                Hide();
                return;
            }
            //Server
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
        }
    }
}
