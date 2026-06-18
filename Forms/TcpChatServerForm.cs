using System;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Network;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TcpChatServerForm : Form
    {
        private readonly TcpChatServer _server =
            new TcpChatServer();

        private int _clientCount = 0;
        private int _messageCount = 0;

        public TcpChatServerForm()
        {
            InitializeComponent();
            WireServerEvents();
        }

        private void btnInitServer_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                _server.Start(
                    (int)numServerPort.Value);

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

        private void WireServerEvents()
        {
            _server.OnStatusChanged +=
                status =>
                SafeInvoke(() =>
                {
                    lblStatus.Text = status;
                });

            _server.OnMessageReceived +=
                message =>
                SafeInvoke(() =>
                {
                    txtChatContent.AppendText(
                        message +
                        Environment.NewLine);

                    _messageCount++;

                    lblTotalMessages.Text =
                        _messageCount.ToString();
                });

            _server.OnClientConnected +=
                endpoint =>
                SafeInvoke(() =>
                {
                    _clientCount++;

                    lblClientCount.Text =
                        _clientCount.ToString();
                });

            _server.OnClientDisconnected +=
                endpoint =>
                SafeInvoke(() =>
                {
                    _clientCount--;

                    if (_clientCount < 0)
                        _clientCount = 0;

                    lblClientCount.Text =
                        _clientCount.ToString();
                });

            _server.OnUserListChanged +=
                users =>
                SafeInvoke(() =>
                {
                    lstUsers.Items.Clear();

                    foreach (string user
                        in users.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(user))
                        {
                            lstUsers.Items.Add(
                                user.Trim());
                        }
                    }
                });
        }

        private void btnSendMessage_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtMessage.Text))
                return;

            try
            {
                string packet =
                    PacketBuilder.BuildMessage(
                        "Server",
                        txtMessage.Text);

                _server.Broadcast(packet);

                txtChatContent.AppendText(
                    "Server: " +
                    txtMessage.Text +
                    Environment.NewLine);

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

        private void SafeInvoke(Action action)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            _server.Stop();
            base.OnFormClosed(e);
        }
    }
}