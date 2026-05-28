using System;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Network;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TcpChatClientForm : Form
    {
        public TcpChatClientForm()
        {
            InitializeComponent();
            WireClientEvents();
        }

        private readonly ChatClientManager _client = new ChatClientManager();

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_client.IsConnected)
            {
                MessageBox.Show(
                    "Client đã kết nối rồi!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                // MỚI
                string username = txtUsername.Text.Trim();
                if (string.IsNullOrWhiteSpace(username))
                {
                    btnConnect.Enabled = true;
                    MessageBox.Show("Vui lòng nhập Username!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                btnConnect.Enabled = false;
                _client.Connect(txtServerIP.Text.Trim(), (int)numServerPort.Value);
            }
            catch (Exception ex)
            {
                btnConnect.Enabled = true;

                MessageBox.Show(
                    "Lỗi kết nối: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void WireClientEvents()
        {
            _client.OnStatusChanged += message => SafeInvoke(() =>
            {

                UpdateStatus(message);

                if (message.StartsWith("Kết nối thất bại"))
                {
                    btnConnect.Enabled = true;
                }

                if (message == "Kết nối thành công.")
    _client.Login(txtUsername.Text.Trim());
            });
            _client.OnMessageReceived += message => SafeInvoke(() => UpdateChatContent(message));
            _client.OnDisconnected += () => SafeInvoke(() =>
            {
                UpdateStatus("Đã ngắt kết nối với Server.");
                btnConnect.Enabled = true;
            });

            _client.OnLoginResult += result => SafeInvoke(() =>
            {
                if (result.StartsWith("OK:"))
                    UpdateChatContent("[System] Đăng nhập thành công!");
                else
                {
                    MessageBox.Show("Đăng nhập thất bại: " + result.Substring(5));
                    _client.Disconnect();
                    btnConnect.Enabled = true;
                }
            });

            _client.OnSystemMessage += msg => SafeInvoke(() =>
                UpdateChatContent("--- " + msg + " ---"));

            _client.OnUserListUpdated += users => SafeInvoke(() =>
                UpdateStatus("Online: " + string.Join(", ", users)));
        }

        private void SafeInvoke(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void UpdateStatus(string s)
        {
            lblStatus.Text = s;
        }

        private void UpdateChatContent(string s)
        {
            txtChatContent.Text += s + "\r\n";
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (!_client.IsConnected)
            {
                MessageBox.Show("Chưa kết nối đến Server!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                return;
            }

            try
            {
                // MỚI
                if (!_client.IsLoggedIn)
                {
                    MessageBox.Show("Chưa đăng nhập xong!");
                    return;
                }
                _client.Send(_client.Username, txtMessage.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi tin nhắn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _client.Disconnect();
            base.OnFormClosed(e);
        }

        private void txtChatContent_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
