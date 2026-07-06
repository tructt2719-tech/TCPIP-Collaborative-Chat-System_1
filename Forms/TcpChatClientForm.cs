using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Client;
using TCPIP_Collaborative_Chat_System.Database;
using TCPIP_Collaborative_Chat_System.Forms;
using TCPIP_Collaborative_Chat_System.Models;
using TCPIP_Collaborative_Chat_System.Network;
using System.Drawing;

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
            CreateEmojiButtons();
            LoadAvatar();
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
        private void CreateEmojiButtons()
        {
            flpEmoji.Controls.Clear();

            foreach (string emoji in _emojis)
            {
                Button btn = new Button();

                btn.Width = 40;
                btn.Height = 40;

                btn.Text = emoji;

                btn.Click += (s, e) =>
                {
                    txtMessage.Text += emoji;

                    txtMessage.Focus();

                    txtMessage.SelectionStart =
                        txtMessage.Text.Length;
                };

                flpEmoji.Controls.Add(btn);
            }
        }
        private readonly ChatClientManager _client = new ChatClientManager();
        private readonly string[] _emojis =
            {
                "😀",
                "😁",
                "😂",
                "🤣",
                "😍",
                "🥰",
                "😎",
                "😭",
                "❤️",
                "👍",
                "👏",
                "🔥"
            };
        private void LoadAvatar()
        {
            string relativePath =
                UserRepository.GetAvatar(_username);

            string fullPath;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                fullPath = Path.Combine(
                    Application.StartupPath,
                    "Avatars",
                    "default.png");
            }
            else
            {
                fullPath = Path.Combine(
                    Application.StartupPath,
                    relativePath);
            }

            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(
                    Application.StartupPath,
                    "Avatars",
                    "default.png");
            }

            if (picAvatar.Image != null)
            {
                picAvatar.Image.Dispose();
                picAvatar.Image = null;
            }

            picAvatar.Image = Image.FromFile(fullPath);
        }
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
            _client.OnFileReceived +=
                (room, sender, fileName, size) =>
                {
                    SafeInvoke(() =>
                    {
                        DialogResult result =
                            MessageBox.Show(
                                $"{sender} vừa gửi file:\n\n" +
                                $"{fileName}\n" +
                                $"({size} bytes)\n\n" +
                                "Bạn có muốn tải không?",
                                "Nhận file",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            _client.DownloadFile(fileName);
                        }
                    });
                };
            _client.OnFileDataReceived +=
                (fileName, data) =>
                {
                    SafeInvoke(() =>
                    {
                        SaveFileDialog dlg =
                            new SaveFileDialog();

                        dlg.FileName = fileName;

                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllBytes(
                                dlg.FileName,
                                data);

                            MessageBox.Show(
                                "Đã tải file thành công!");
                        }
                    });
                };
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
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _client.SendFile(dlg.FileName);
            }
        }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnEmoji_Click(object sender, EventArgs e)
        {
            flpEmoji.Visible = !flpEmoji.Visible;
        }

        private void btnChangeAvatar_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            string avatarFolder =
                Path.Combine(
                    Application.StartupPath,
                    "Avatars");

            if (!Directory.Exists(avatarFolder))
            {
                Directory.CreateDirectory(
                    avatarFolder);
            }

            string extension =
                Path.GetExtension(dlg.FileName);

            string newFileName =
                _username + extension;

            string destination =
                Path.Combine(
                    avatarFolder,
                    newFileName);

            File.Copy(
                dlg.FileName,
                destination,
                true);

           string relativePath = Path.Combine("Avatars", newFileName);
            UserRepository.UpdateAvatar(_username, relativePath);
            if (picAvatar.Image != null)
            {
                picAvatar.Image.Dispose();
                picAvatar.Image = null;
            }
            picAvatar.Image = Image.FromFile(destination);
            MessageBox.Show("Đổi Avatar thành công!");
        }

        private void lblReplySender_Click(object sender, EventArgs e)
        {

        }
    }
}
