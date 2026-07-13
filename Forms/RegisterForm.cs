using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Database;

namespace TCPIP_Collaborative_Chat_System.Forms
{
    public partial class RegisterForm : Form
    {
        public string RegisteredUsername
        {
            get;
            private set;
        }
        public RegisterForm()
        {
            InitializeComponent();

            AcceptButton = btnRegister;
        }

        private void Register()
        {
            string username = txtUserName.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Vui lòng nhập Username");
                txtUserName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập Password");
                txtPassword.Focus();
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Xác nhận mật khẩu không đúng");
                txtConfirmPassword.Clear();
                txtConfirmPassword.Focus();
                return;
            }

            if (UserRepository.UserExists(username))
            {
                MessageBox.Show("Username đã tồn tại");
                txtUserName.Focus();
                return;
            }
            string hash = PasswordHasher.Hash(password);
            bool ok = UserRepository.AddUser(username, hash);

            if (ok)
            {
                RegisteredUsername = username;
                MessageBox.Show("Đăng ký thành công!");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Không thể tạo tài khoản.");
            }
        }
        private void btnRegister_Click(object sender, EventArgs e)
        {
            Register();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
