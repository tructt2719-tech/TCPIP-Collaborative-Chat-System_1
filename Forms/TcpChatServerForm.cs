using System;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Network;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System
{
    // TcpChatServerForm: Dashboard quản lý TCP Server.
    // Controls từ Designer: btnInitServer, numServerPort, lblStatus,
    //   lblClientCount, lblTotalMessages, lstUsers, txtChatContent,
    //   txtMessage, btnSendMessage.
    public partial class TcpChatServerForm : Form
    {
        private readonly TcpChatServer _server = new TcpChatServer();

        private int _clientCount = 0;
        private int _messageCount = 0;

        public TcpChatServerForm()
        {
            InitializeComponent();
            WireServerEvents();
        }

        // ===== Khởi động Server =====

        private void btnInitServer_Click(object sender, EventArgs e)
        {
            try
            {
                _server.Start((int)numServerPort.Value);
                btnInitServer.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===== Broadcast từ Server đến tất cả client =====

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
                return;

            try
            {
                // Tạo messageId tạm cho tin nhắn từ Server
                string msgId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string packet = PacketBuilder.BuildMessage("Server", "Server", txtMessage.Text);
                _server.Broadcast(packet);

                txtChatContent.AppendText("Server: " + txtMessage.Text + Environment.NewLine);
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Broadcast Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===== Kết nối sự kiện từ TcpChatServer vào UI =====

        private void WireServerEvents()
        {
            // Trạng thái server thay đổi (ví dụ: "Server chạy ở port 9000")
            _server.OnStatusChanged += status =>
                SafeInvoke(() =>
                {
                    lblStatus.Text = status;
                });

            // Nhận / hiển thị log tin nhắn và sự kiện trên chat log
            _server.OnMessageReceived += message =>
                SafeInvoke(() =>
                {
                    txtChatContent.AppendText(message + Environment.NewLine);
                    _messageCount++;
                    lblTotalMessages.Text = _messageCount.ToString();
                });

            // Client mới kết nối → tăng đếm
            _server.OnClientConnected += endpoint =>
                SafeInvoke(() =>
                {
                    _clientCount++;
                    lblClientCount.Text = _clientCount.ToString();
                });

            // Client ngắt kết nối → giảm đếm
            _server.OnClientDisconnected += endpoint =>
                SafeInvoke(() =>
                {
                    _clientCount = Math.Max(0, _clientCount - 1);
                    lblClientCount.Text = _clientCount.ToString();
                });

            // Danh sách user online thay đổi → cập nhật lstUsers
            _server.OnUserListChanged += users =>
                SafeInvoke(() =>
                {
                    lstUsers.Items.Clear();
                    foreach (string user in users.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(user))
                            lstUsers.Items.Add(user.Trim());
                    }
                });
        }

        // ===== Helpers =====

        private void SafeInvoke(Action action)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _server.Stop();
            base.OnFormClosed(e);
        }
    }
}
