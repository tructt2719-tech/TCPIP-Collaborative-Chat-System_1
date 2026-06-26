using System;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Database;
using TCPIP_Collaborative_Chat_System.Network;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TcpChatServerForm : Form
    {
        public TcpChatServerForm()
        {
            InitializeComponent();
            WireServerEvents();
        }
        private readonly TcpChatServer _server = new TcpChatServer();
        private int _clientCount;

        private void btnInitServer_Click(object sender, EventArgs e)
        {
            try
            {
                _server.Start((int)numServerPort.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void WireServerEvents()
        {
            _server.OnStatusChanged += status => SafeInvoke(() => UpdateStatus(status));
            _server.OnMessageReceived += message => SafeInvoke(() => UpdateChatContent(message));
            _server.OnClientConnected += endpoint =>
                SafeInvoke(() =>
                {
                    _clientCount++;
                    UpdateStatus($"Client connected: {endpoint}");
                });
            _server.OnClientDisconnected += endpoint =>
                SafeInvoke(() =>
                {
                    _clientCount = Math.Max(0, _clientCount - 1);
                    UpdateStatus($"Client disconnected: {endpoint}");
                });
        }

        private void SafeInvoke(Action action)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore callbacks after the form is closed.
            }
            catch (InvalidOperationException)
            {
                // Ignore callbacks when handle is no longer valid.
            }
        }

        private void UpdateStatus(string s)
        {
            lblStatus.Text = s;
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (_clientCount == 0)
            {
                MessageBox.Show("Chưa có Client nào kết nối!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                return;
            }

            try
            {
                string packet = PacketBuilder.BuildMessage("Server", txtMessage.Text);
                _server.Broadcast(packet);
                UpdateChatContent("Server: " + txtMessage.Text);
                txtMessage.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi tin nhắn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UpdateChatContent(string s)
        {
            txtChatContent.Text += s + "\r\n";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _server.Stop();
            base.OnFormClosed(e);
        }

        private void txtChatContent_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
