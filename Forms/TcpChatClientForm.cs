using System;
using System.Net;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Client;
using TCPIP_Collaborative_Chat_System.Forms;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Network;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TcpChatClientForm : Form
    {
        private string _currentRoom = "";
        private readonly string _username;
        private readonly string _password;
        private readonly bool _remember;
        public TcpChatClientForm(string username, string password, bool remember)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _remember = remember;
            txtUsername.Text = username;
            txtUsername.Enabled = false;
            WireClientEvents();
            LoadConnectionSettings();
        }
        private void LoadConnectionSettings()
        {
            numServerPort.Value = 12345;
            if (!SettingsManager.Exists())
                return;
            string ip = SettingsManager.Read("ServerIP");
            if (!string.IsNullOrWhiteSpace(ip))
            {
                txtServerIP.Text = ip;
            }
            string port = SettingsManager.Read("Port");
            int p;
            if (int.TryParse(port, out p))
            {
                numServerPort.Value = p;
            }
        }

        private readonly ChatClientManager _client = new ChatClientManager();

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_client.IsConnected)
            {
                MessageBox.Show("Client đã kết nối rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            try
            {         
                if (string.IsNullOrWhiteSpace(txtServerIP.Text))
                {
                    MessageBox.Show("Vui lòng nhập địa chỉ IP Server.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtServerIP.Focus();

                    return;
                }

                IPAddress ip;

                if (!IPAddress.TryParse(txtServerIP.Text.Trim(), out ip))
                {
                    MessageBox.Show("Địa chỉ IP không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtServerIP.Focus();

                    return;
                }

                _client.Connect(txtServerIP.Text.Trim(), (int)numServerPort.Value);
            }
            catch (Exception ex)
            {
                btnConnect.Enabled = true;
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (_remember)
                    {
                        SettingsManager.Save(_username, true, txtServerIP.Text.Trim(), (int)numServerPort.Value);
                    }
                    else
                    {
                        SettingsManager.Delete();
                    }
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
                        string[] data = roomInfo.Split(',');
                        string roomName =  data[0];
                        bool isPrivate = data.Length > 1 && data[1] == "PRIVATE";
                        lstRooms.Items.Add(
                            new RoomItem
                            {
                                Name = roomName,
                                IsPrivate = isPrivate
                            });
                    }
                });
            };
            _client.OnDeleteRoomResult += result => SafeInvoke(() =>
            {
                if (result.StartsWith("OK:"))
                {
                    string roomName = result.Substring(3);
                    _currentRoom = "";
                    UpdateChatContent($"Phòng '{roomName}' đã xóa thành công");
                }
                else
                {
                    string reason = result.Length > 5 ? result.Substring(5) : result;
                    MessageBox.Show(reason, "Không thể xóa phòng", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            });
            _client.OnRoomDeleted += roomName => SafeInvoke(() =>
            {
                if (_currentRoom == roomName)
                    _currentRoom = "";

                txtChatContent.Clear();
                UpdateChatContent($"Phòng '{roomName}' đã bị chủ phòng xóa");
                UpdateStatus($"Phòng '{roomName}' đã bị xóa");
            });

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
            txtChatContent.AppendText(s + "\r\n");
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
        private void btnDeleteRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phòng muốn xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            RoomItem room = (RoomItem)lstRooms.SelectedItem;
            var confirm = MessageBox.Show($"Bạn có muốn xóa phòng \"{room.Name}\" không?\n\n" +
                "Yes\n" + "No", "Xác nhận xóa phòng", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm == DialogResult.Yes)
                _client.DeleteRoom(room.Name);
        }
    }
}
