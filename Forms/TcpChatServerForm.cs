using System;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Database;
using TCPIP_Collaborative_Chat_System.Network;
using TCPIP_Collaborative_Chat_System.Shared;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

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
        private string GetLocalIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                // Bỏ qua VMware
                if (ni.Description.Contains("VMware"))
                    continue;

                // Bỏ qua Tailscale
                if (ni.Description.Contains("Tailscale"))
                    continue;

                IPInterfaceProperties ipProps = ni.GetIPProperties();

                foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }

            return "Không tìm thấy";
        }
        private void btnInitServer_Click(object sender, EventArgs e)
        {
            try
            {
                _server.Start((int)numServerPort.Value);
                ShowServerInformation();
                UpdateChatContent($"Server đang lắng nghe tại {GetLocalIPv4()}:{numServerPort.Value}");
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
        private void ShowServerInformation()
        {
            string ip = GetLocalIPv4();
            lblServerIP.Text = "IP : " + ip;
            lblStatus.Text = $"Server đang chạy - Client kết nối tới {ip}:{numServerPort.Value}";
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
