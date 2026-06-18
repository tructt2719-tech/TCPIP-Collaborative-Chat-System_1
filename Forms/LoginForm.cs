using System;
using System.Net;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Network;
using TCPIP_Collaborative_Chat_System.Properties;

namespace TCPIP_Collaborative_Chat_System.Forms
{
    public partial class LoginForm : Form
    {
        private TcpChatClientForm _clientForm;

        public LoginForm()
        {
            InitializeComponent();
            LoadSavedSettings();
        }

        private void LoadSavedSettings()
        {
            var settings = Settings.Default;

            if (!settings.RememberMe)
                return;

            chkRemember.Checked = true;
            txtUsername.Text = settings.SavedUsername ?? string.Empty;
            txtIP.Text = string.IsNullOrWhiteSpace(settings.SavedServerIp)
                ? "127.0.0.1"
                : settings.SavedServerIp;

            if (settings.SavedServerPort >= 1 && settings.SavedServerPort <= 65535)
                numPort.Value = settings.SavedServerPort;

            txtKey.Text = settings.SavedAesKey ?? string.Empty;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(out string error))
            {
                MessageBox.Show(
                    error,
                    "Lỗi đăng nhập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SetLoginButtonsEnabled(false);

            try
            {
                var client = new ChatClientManager();

                bool success = client.TryLogin(
                    txtIP.Text.Trim(),
                    (int)numPort.Value,
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    out string loginError);

                if (!success)
                {
                    MessageBox.Show(
                        loginError ?? "Đăng nhập thất bại",
                        "Lỗi đăng nhập",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                SaveRememberSettings();

                _clientForm = new TcpChatClientForm();
                _clientForm.SetLoginInfo(
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    txtIP.Text.Trim(),
                    (int)numPort.Value,
                    txtKey.Text.Trim(),
                    autoConnect: true);

                _clientForm.FormClosed += ClientForm_FormClosed;
                _clientForm.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi đăng nhập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetLoginButtonsEnabled(true);
            }
        }

        private void SetLoginButtonsEnabled(bool enabled)
        {
            btnLogin.Enabled = enabled;
            btnRegister.Enabled = enabled;
            UseWaitCursor = !enabled;
        }

        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _clientForm.FormClosed -= ClientForm_FormClosed;
            _clientForm = null;
            Show();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateServerEndpoint(out string error))
            {
                MessageBox.Show(
                    error,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (var registerForm = new RegisterForm(
                txtIP.Text.Trim(),
                (int)numPort.Value))
            {
                registerForm.ShowDialog(this);
            }
        }

        private bool ValidateInputs(out string error)
        {
            if (!ValidateUsername(txtUsername.Text, out error))
                return false;

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                error = "Password không được rỗng";
                return false;
            }

            return ValidateServerEndpoint(out error);
        }

        private bool ValidateServerEndpoint(out string error)
        {
            error = null;

            if (!IPAddress.TryParse(txtIP.Text.Trim(), out _))
            {
                error = "IP Address không hợp lệ";
                return false;
            }

            int port = (int)numPort.Value;
            if (port < 1 || port > 65535)
            {
                error = "Port phải từ 1 đến 65535";
                return false;
            }

            return true;
        }

        private static bool ValidateUsername(string username, out string error)
        {
            return UserStore.ValidateUsername(username, out error);
        }

        private void SaveRememberSettings()
        {
            var settings = Settings.Default;
            settings.RememberMe = chkRemember.Checked;

            if (chkRemember.Checked)
            {
                settings.SavedUsername = txtUsername.Text.Trim();
                settings.SavedServerIp = txtIP.Text.Trim();
                settings.SavedServerPort = (int)numPort.Value;
                settings.SavedAesKey = txtKey.Text.Trim();
            }
            else
            {
                settings.SavedUsername = string.Empty;
                settings.SavedServerIp = "127.0.0.1";
                settings.SavedServerPort = 9000;
                settings.SavedAesKey = string.Empty;
            }

            settings.Save();
        }
    }
}