using System;
using System.Net;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Network;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TcpChatClientForm : Form
    {
        private readonly ChatClientManager _client = new ChatClientManager();

        private string _password = string.Empty;
        private string _aesKey = string.Empty;
        private bool _autoConnect;
        private bool _autoConnectTriggered;
        private string _replyMessage = string.Empty;

        public TcpChatClientForm()
        {
            InitializeComponent();
            WireClientEvents();
            SetChatControlsEnabled(false);
        }

        public void SetLoginInfo(
            string username,
            string password,
            string serverIp,
            int port,
            string aesKey,
            bool autoConnect = false)
        {
            txtUsername.Text = username ?? string.Empty;
            txtServerIP.Text = serverIp ?? string.Empty;
            numServerPort.Value = Math.Max(1, Math.Min(65535, port));
            _password = password ?? string.Empty;
            _aesKey = aesKey ?? string.Empty;
            _autoConnect = autoConnect;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_autoConnect && !_autoConnectTriggered)
            {
                _autoConnectTriggered = true;
                ConnectInternal();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectInternal();
        }

        private void ConnectInternal()
        {
            try
            {
                if (_client.IsConnected)
                {
                    MessageBox.Show("Đã kết nối rồi!", "Thông báo");
                    return;
                }

                string username = txtUsername.Text.Trim();
                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Nhập Username!", "Lỗi");
                    return;
                }

                if (!IPAddress.TryParse(txtServerIP.Text.Trim(), out _))
                {
                    MessageBox.Show("IP Address không hợp lệ", "Lỗi");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_password))
                {
                    MessageBox.Show(
                        "Thiếu mật khẩu. Vui lòng đăng nhập lại từ LoginForm.",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                SetConnectionInputsEnabled(false);
                SetChatControlsEnabled(false);
                lblStatus.Text = "Đang kết nối...";

                _client.Connect(
                    txtServerIP.Text.Trim(),
                    (int)numServerPort.Value);
            }
            catch (Exception ex)
            {
                SetConnectionInputsEnabled(true);
                lblStatus.Text = "Kết nối thất bại";
                MessageBox.Show(ex.Message, "Lỗi kết nối");
            }
        }

        private void WireClientEvents()
        {
            _client.OnStatusChanged += msg =>
                SafeInvoke(() =>
                {
                    lblStatus.Text = msg;

                    if (msg == "Kết nối thành công.")
                    {
                        lblStatus.Text = "Đang đăng nhập...";
                        _client.Login(txtUsername.Text.Trim(), _password);
                    }
                });

            _client.OnLoginResult += result =>
                SafeInvoke(() =>
                {
                    if (result.StartsWith("OK:"))
                    {
                        lblStatus.Text = "Đăng nhập thành công";
                        SetConnectionInputsEnabled(false);
                        SetChatControlsEnabled(true);
                    }
                    else if (result.StartsWith("FAIL:"))
                    {
                        string reason = result.Length > 5
                            ? result.Substring(5)
                            : "Đăng nhập thất bại";

                        SetChatControlsEnabled(false);

                        MessageBox.Show(
                            reason,
                            "Đăng nhập thất bại",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        _client.Disconnect();

                        btnConnect.Enabled = true;
                        lblStatus.Text = "Disconnected";
                        SetConnectionInputsEnabled(true);
                    }
                });

            _client.OnMessageReceived += msg =>
                SafeInvoke(() =>
                {
                    txtChatContent.AppendText(msg + Environment.NewLine);
                });

            _client.OnSystemMessage += msg =>
                SafeInvoke(() =>
                {
                    txtChatContent.AppendText("[System] " + msg + Environment.NewLine);
                });

            _client.OnDisconnected += () =>
                SafeInvoke(() =>
                {
                    lblStatus.Text = "Disconnected";
                    SetConnectionInputsEnabled(true);
                    SetChatControlsEnabled(false);
                });

            _client.OnUserListUpdated += users =>
                SafeInvoke(() =>
                {
                    lstUsers.Items.Clear();
                    foreach (var user in users)
                        lstUsers.Items.Add(user);
                });
        }

        private void SetConnectionInputsEnabled(bool enabled)
        {
            txtServerIP.Enabled = enabled;
            txtUsername.Enabled = enabled;
            numServerPort.Enabled = enabled;
            btnConnect.Enabled = enabled;
        }

        private void SetChatControlsEnabled(bool enabled)
        {
            txtMessage.Enabled = enabled;
            btnSendMessage.Enabled = enabled;
            btnEmoji.Enabled = enabled;
            btnReply.Enabled = enabled;
            btnForward.Enabled = enabled;
            btnFile.Enabled = enabled;
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (!_client.IsConnected || !_client.IsLoggedIn)
            {
                MessageBox.Show("Chưa kết nối hoặc chưa đăng nhập.");
                return;
            }

            string msg = txtMessage.Text.Trim();
            if (msg == "")
                return;

            if (_replyMessage != "")
            {
                msg = "[Reply] " + _replyMessage + " => " + msg;
            }

            _client.Send(_client.Username, msg);
            txtMessage.Clear();
            _replyMessage = "";
        }

        private void btnEmoji_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            string[] emojis = { "😀", "😂", "😍", "👍", "❤️", "🎉" };

            foreach (string emoji in emojis)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(emoji);
                item.Click += (s, ev) => { txtMessage.Text += emoji; };
                menu.Items.Add(item);
            }

            menu.Show(btnEmoji, 0, btnEmoji.Height);
        }

        private void btnReply_Click(object sender, EventArgs e)
        {
            _replyMessage = "Message";
            txtMessage.Text = "[Reply] ";
            txtMessage.Focus();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Forward feature");
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtMessage.Text = "[FILE] " + dlg.FileName;
            }
        }

        private void SafeInvoke(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
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
        private void ReplyMessage(string messageId, string originalSender)
        {
            if (!_client.IsConnected || !_client.IsLoggedIn) return;
            string replyContent = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(replyContent)) return;
            string packet = $"RELY_MESSAGE|{Guid.NewGuid()}|{_client.Username}|{replyContent}|{messageId}|{originalSender}\n";
            _client._socket?.Send(System.Text.Encoding.UTF8.GetBytes(packet));
            txtMessage.Text = "";
        }


    }
}