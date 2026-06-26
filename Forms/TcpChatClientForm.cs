using System;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Forms;
using TCPIP_Collaborative_Chat_System.Network;
using TCPIP_Collaborative_Chat_System.Models;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TcpChatClientForm : Form
    {
        private string _currentRoom = "";
        private readonly string _username;
        private readonly string _password;
        public TcpChatClientForm(string username, string password)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            txtUsername.Text = username;
            txtUsername.Enabled = false;
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
                {
                    _client.Login(_username, _password);
                }
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

            _client.OnRoomMessage += msg => SafeInvoke(() =>
                UpdateChatContent(msg));

            _client.OnRoomHistory += msg => SafeInvoke(() =>
                UpdateChatContent(msg));

            _client.OnRoomUserJoined += msg => SafeInvoke(() =>
                UpdateChatContent("[ROOM] " + msg));

            _client.OnRoomUserLeft += msg =>SafeInvoke(() =>
                UpdateChatContent("[ROOM] " + msg));

            _client.OnRoomListReceived += rooms =>
            {
                SafeInvoke(() =>
                {
                    lstRooms.Items.Clear();

                    foreach (var roomInfo in rooms)
                    {
                        string[] data =
                            roomInfo.Split(',');

                        string roomName =
                            data[0];

                        bool isPrivate =
                            data.Length > 1 &&
                            data[1] == "PRIVATE";

                        lstRooms.Items.Add(
                            new RoomItem
                            {
                                Name = roomName,
                                IsPrivate = isPrivate
                            });
                    }
                });
            };

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
                if (!_client.IsLoggedIn)
                {
                    MessageBox.Show("Chưa đăng nhập xong!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_currentRoom))
                {
                    MessageBox.Show("Bạn chưa tham gia phòng nào");
                    return;
                }
                _client.SendRoomMessage(_currentRoom, txtMessage.Text.Trim());
                txtMessage.Clear();
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnCreateRoom_Click(object sender, EventArgs e)
        {
            CreateRoomForm frm = new CreateRoomForm();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _client.CreateRoom(frm.RoomName, frm.MaxUsers, frm.IsPrivate, frm.Password);
            }
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null)
            {
                MessageBox.Show("Chọn phòng trước");
                return;
            }
            RoomItem room = (RoomItem)lstRooms.SelectedItem;
            string roomName = room.Name;
            string password = "";

            if (room.IsPrivate)
            {
                password = txtRoomName.Text.Trim();

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu");
                    return;
                }
            }

            txtChatContent.Clear();
            _client.JoinRoom(roomName, password);
            _currentRoom = room.Name;
        }

        private void btnLeaveRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phòng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RoomItem room = (RoomItem)lstRooms.SelectedItem;

            if (MessageBox.Show($"Bạn có chắc muốn rời phòng '{room.Name}' ?", "Xác nhận rời phòng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _client.LeaveRoom(room.Name);
                _currentRoom = "";
            }
        }
    }
}
