using System;
using System.IO;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Network;

namespace TCPIP_Collaborative_Chat_System.Forms
{
    public partial class RegisterForm : Form
    {
        private const int MaxAvatarBytes = 50 * 1024;

        private readonly string _serverIp;
        private readonly int _serverPort;
        private string _avatarPath;

        public RegisterForm(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(out string validationError))
            {
                MessageBox.Show(
                    validationError,
                    "Lỗi đăng ký",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnRegister.Enabled = false;
            btnBack.Enabled = false;

            try
            {
                string avatarBase64 = GetAvatarBase64();
                var client = new ChatClientManager();

                bool success = client.TryRegister(
                    _serverIp,
                    _serverPort,
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    txtEmail.Text.Trim(),
                    avatarBase64,
                    out string error);

                if (success)
                {
                    MessageBox.Show(
                        "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        error ?? "Đăng ký thất bại",
                        "Lỗi đăng ký",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi đăng ký",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnBack.Enabled = true;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnChooseAvatar_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var fileInfo = new FileInfo(dlg.FileName);
                    if (fileInfo.Length > MaxAvatarBytes)
                    {
                        MessageBox.Show(
                            "Ảnh quá lớn. Vui lòng chọn ảnh nhỏ hơn 50KB.",
                            "Cảnh báo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    _avatarPath = dlg.FileName;
                    picAvatar.ImageLocation = dlg.FileName;
                }
            }
        }

        private void btnRemoveAvatar_Click(object sender, EventArgs e)
        {
            _avatarPath = null;
            picAvatar.Image = null;
            picAvatar.ImageLocation = null;
        }

        private bool ValidateInputs(out string error)
        {
            error = null;

            if (!UserStore.ValidateUsername(txtUsername.Text, out error))
                return false;

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                error = "Password không được rỗng";
                return false;
            }

            if (txtPassword.Text.Length < 4)
            {
                error = "Password phải có ít nhất 4 ký tự";
                return false;
            }

            if (!string.Equals(txtPassword.Text, txtConfirm.Text, StringComparison.Ordinal))
            {
                error = "Xác nhận password không khớp";
                return false;
            }

            string email = txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                error = "Email không hợp lệ";
                return false;
            }

            if (email.Contains("|") || email.Contains("\n") || email.Contains("\r"))
            {
                error = "Email chứa ký tự không hợp lệ";
                return false;
            }

            return true;
        }

        private string GetAvatarBase64()
        {
            if (string.IsNullOrWhiteSpace(_avatarPath) || !File.Exists(_avatarPath))
                return string.Empty;

            byte[] bytes = File.ReadAllBytes(_avatarPath);
            return Convert.ToBase64String(bytes);
        }
    }
}