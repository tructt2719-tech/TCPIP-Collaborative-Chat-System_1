using System;
using System.Net;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Client;
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
        private Label lblTyping;
        public TcpChatClientForm(string username, string password, bool remember)
        {
            InitializeComponent();
            lblTyping = new Label();
            lblTyping.AutoSize = true;
            lblTyping.ForeColor = Color.Gray;
            lblTyping.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblTyping.Visible = false;
            lblTyping.Location = new Point(10, pnlChat.Bottom + 5);

            this.Controls.Add(lblTyping);
            typingTimer.Interval = 2000; 
            typingTimer.Tick += TypingTimer_Tick;
            pnlChat.Resize += (s, e) =>
            {
                foreach (Control c in pnlChat.Controls)
                {
                    c.Width = pnlChat.ClientSize.Width - 25;
                }
            };
            _username = username;
            _password = password;
            _remember = remember;
            txtUsername.Text = username;
            txtUsername.Enabled = false;
            WireClientEvents();
            LoadConnectionSettings();
            txtMessage.KeyDown += TxtMessage_KeyDown;
            txtMessage.TextChanged += TxtMessage_TextChanged;
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
        private Timer typingTimer = new Timer();
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
            {
                AddNotification(msg);
            });

            _client.OnUserListUpdated += users =>
            {
                SafeInvoke(() =>
                {
                    lstOnlineUsers.Items.Clear();

                    foreach (string user in users)
                    {
                        lstOnlineUsers.Items.Add("🟢 " + user);
                    }

                    UpdateStatus("Online: " + users.Count);
                });
            };

            _client.OnRoomMessage += msg => SafeInvoke(() =>
            {
                string[] data = msg.Split(new[] { ':' }, 2);

                if (data.Length == 2)
                {
                    AddMessage(data[0].Trim(), data[1].Trim(), data[0].Trim() == _username);
                }
                else
                {
                    AddNotification(msg);
                }
            });

            _client.OnRoomHistory += msg => SafeInvoke(() =>
                UpdateChatContent(msg));

            _client.OnRoomUserJoined += msg => SafeInvoke(() =>
            {
                AddNotification(msg);});
            
            _client.OnRoomUserLeft += msg => SafeInvoke(() =>
            {
                AddNotification(msg);});
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
            _client.OnTypingReceived += username =>
            {
                SafeInvoke(() =>
                {
                    lblTyping.Text = username + " đang nhập...";
                    lblTyping.Visible = true;

                    typingTimer.Stop();
                    typingTimer.Start();
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
            string user = "System";
            string message = s;

            int index = s.IndexOf(':');

            if (index > 0)
            {
                user = s.Substring(0, index).Trim();
                message = s.Substring(index + 1).Trim();
            }

            bool me = user.Equals(_username, StringComparison.OrdinalIgnoreCase);

            AddMessage(user, message, me);
            pnlChat.VerticalScroll.Value = pnlChat.VerticalScroll.Maximum;
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
                AddMessage(_username, txtMessage.Text.Trim(), true);
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
            pnlChat.Controls.Clear();
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
        private void AddNotification(string text)
        {
            Label lbl = new Label();

            lbl.AutoSize = true;
            lbl.ForeColor = Color.Gray;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lbl.Margin = new Padding(10);
            lbl.Text = "🔔 " + text;

            pnlChat.Controls.Add(lbl);
            pnlChat.ScrollControlIntoView(lbl);
        }
        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnSendMessage.PerformClick();
            }
        }
        private void AddMessage(string user, string message, bool me)
        {
            // Dòng chứa avatar + bubble
            FlowLayoutPanel row = new FlowLayoutPanel();
            row.FlowDirection = me ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            row.WrapContents = false;
            row.AutoSize = true;
            row.Width = pnlChat.ClientSize.Width - 25;
            row.Margin = new Padding(5);

            // Avatar
            PictureBox avatar = new PictureBox();
            avatar.Size = new Size(40, 40);
            avatar.BackColor = Color.SteelBlue;
            avatar.Margin = new Padding(5);

            // Bubble
            Panel bubble = new Panel();
            bubble.AutoSize = true;
            bubble.MaximumSize = new Size(280, 0);
            bubble.Padding = new Padding(10);
            bubble.Margin = new Padding(5);

            bubble.BackColor = me
                ? Color.FromArgb(0, 120, 215)
                : Color.Gainsboro;

            // Layout trong bubble
            FlowLayoutPanel content = new FlowLayoutPanel();
            content.FlowDirection = FlowDirection.TopDown;
            content.WrapContents = false;
            content.AutoSize = true;

            // Username
            Label lblUser = new Label();
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblUser.ForeColor = me ? Color.White : Color.Black;
            lblUser.Text = user;

            // Message
            Label lblMessage = new Label();
            lblMessage.AutoSize = true;
            lblMessage.MaximumSize = new Size(240, 0);
            lblMessage.Font = new Font("Segoe UI", 10);
            lblMessage.ForeColor = me ? Color.White : Color.Black;
            lblMessage.Text = message;

            // Time
            Label lblTime = new Label();
            lblTime.AutoSize = true;
            lblTime.Font = new Font("Segoe UI", 7);
            lblTime.ForeColor = me ? Color.WhiteSmoke : Color.Gray;
            lblTime.Text = DateTime.Now.ToString("HH:mm");

            content.Controls.Add(lblUser);
            content.Controls.Add(lblMessage);
            content.Controls.Add(lblTime);

            bubble.Controls.Add(content);

            row.Controls.Add(avatar);
            row.Controls.Add(bubble);

            pnlChat.Controls.Add(row);

            pnlChat.ScrollControlIntoView(row);
        }
        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            lblTyping.Visible = false;
            typingTimer.Stop();
        }
        private void TxtMessage_TextChanged(object sender, EventArgs e)
        {
            if (_client.IsConnected && !string.IsNullOrEmpty(_currentRoom))
            {
                _client.SendTyping(_currentRoom);
            }
        }
    }

    }
