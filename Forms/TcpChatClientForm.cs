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
                MessageBox.Show("Client đã kết nối rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectInternal();
        }

        private void ConnectInternal()
        {
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

            _client.OnRoomSystemMessage += (roomName, message) =>
                SafeInvoke(() =>
                {
                    // Chỉ hiện thông báo của room đang xem để tránh nhiễu giữa các phòng khác
                    if (roomName.Equals(_client.CurrentRoom, StringComparison.OrdinalIgnoreCase))
                        AppendPlainLine($"[System] {message}");
                });

            _client.OnRoomError += (code, detail) =>
                SafeInvoke(() =>
                {
                    AppendPlainLine($"[Lỗi] {code}: {detail}");
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

        // ===== Gửi tin nhắn (thường hoặc reply) =====

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            SendCurrentMessage();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendCurrentMessage();
            }
        }

        private void SendCurrentMessage()
        {
            if (!_client.IsConnected || !_client.IsLoggedIn)
            {
                MessageBox.Show("Chưa kết nối hoặc chưa đăng nhập.");
                return;
            }

            string msg = txtMessage.Text.Trim();
            if (msg == "")
                return;

            string room = _client.CurrentRoom;

            if (_pendingReply != null)
            {
                if (!string.IsNullOrEmpty(room))
                    _client.SendRoomReply(room, msg, _pendingReply.MessageId);
                else
                    _client.SendReply(msg, _pendingReply.MessageId);

                ClearPendingReply();
            }
            else
            {
                if (!string.IsNullOrEmpty(room))
                    _client.SendRoomMessage(room, msg);
                else
                    _client.Send(_client.Username, msg);
            }

            txtMessage.Clear();
        }

        // ===== Emoji: chèn ký tự vào ô nhập, hoặc react vào message đang chọn =====

        private void btnEmoji_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            string[] emojis = { "😀", "😂", "😍", "👍", "❤️", "🎉" };

            bool reactToSelected = _selectedMessage != null;

            foreach (string emoji in emojis)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(emoji);
                item.Click += (s, ev) =>
                {
                    if (reactToSelected)
                    {
                        string room = _client.CurrentRoom;
                        if (!string.IsNullOrEmpty(room))
                            _client.SendRoomEmojiReaction(room, _selectedMessage.MessageId, emoji);
                        else
                            _client.SendEmojiReaction(_selectedMessage.MessageId, emoji);
                    }
                    else
                    {
                        txtMessage.Text += emoji;
                        txtMessage.SelectionStart = txtMessage.Text.Length;
                        txtMessage.Focus();
                    }
                };
                menu.Items.Add(item);
            }

            if (reactToSelected)
            {
                menu.Items.Add(new ToolStripSeparator());
                var hint = new ToolStripMenuItem($"React vào tin nhắn của {_selectedMessage.Sender}") { Enabled = false };
                menu.Items.Add(hint);
            }

            menu.Show(btnEmoji, 0, btnEmoji.Height);
        }

        // ===== Reply: yêu cầu đã chọn message (right-click / double-click) =====

        private void btnReply_Click(object sender, EventArgs e)
        {
            if (_selectedMessage == null)
            {
                MessageBox.Show(
                    "Hãy double-click hoặc right-click vào một tin nhắn trong khung chat để chọn tin nhắn cần Reply.",
                    "Chưa chọn tin nhắn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            BeginReply(_selectedMessage);
        }

        private void BeginReply(ChatMessageInfo target)
        {
            _pendingReply = target;
            lblReplyPreview.Text = $"↩ Đang trả lời {target.Sender}: {Truncate(target.Content, 60)}";
            lblReplyPreview.Visible = true;
            txtMessage.Focus();
        }

        private void btnCancelReply_Click(object sender, EventArgs e)
        {
            ClearPendingReply();
        }

        private void ClearPendingReply()
        {
            _pendingReply = null;
            lblReplyPreview.Visible = false;
        }

        // ===== Forward: yêu cầu đã chọn message (right-click / double-click) =====

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (_selectedMessage == null)
            {
                MessageBox.Show(
                    "Hãy double-click hoặc right-click vào một tin nhắn trong khung chat để chọn tin nhắn cần Forward.",
                    "Chưa chọn tin nhắn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!_client.IsConnected || !_client.IsLoggedIn)
            {
                MessageBox.Show("Chưa kết nối hoặc chưa đăng nhập.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Chuyển tiếp tin nhắn của {_selectedMessage.Sender}: \"{Truncate(_selectedMessage.Content, 80)}\" ?",
                "Forward",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string room = _client.CurrentRoom;
                if (!string.IsNullOrEmpty(room))
                    _client.SendRoomForward(room, _selectedMessage.MessageId);
                else
                    _client.SendForward(_selectedMessage.MessageId);
            }
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtMessage.Text = "[FILE] " + dlg.FileName;
            }
        }

        // ===== Chọn message bằng double-click / right-click trên RichTextBox =====

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

        }
        private void SafeInvoke(Action action)
        {
            var entry = FindLineAt(txtChatContent.GetCharIndexFromPosition(e.Location));
            if (entry == null) return;

            SelectMessage(entry);
            BeginReply(entry.Message);
        }

        private void TxtChatContent_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            int charIndex = txtChatContent.GetCharIndexFromPosition(e.Location);
            var entry = FindLineAt(charIndex);
            if (entry == null) return;

            SelectMessage(entry);
            ShowMessageContextMenu(entry.Message, e.Location);
        }

        private void ShowMessageContextMenu(ChatMessageInfo message, Point location)
        {
            var menu = new ContextMenuStrip();

            var replyItem = new ToolStripMenuItem("↩ Reply");
            replyItem.Click += (s, e) => BeginReply(message);
            menu.Items.Add(replyItem);

            var forwardItem = new ToolStripMenuItem("➜ Forward");
            forwardItem.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
                    $"Chuyển tiếp tin nhắn của {message.Sender}: \"{Truncate(message.Content, 80)}\" ?",
                    "Forward",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    string room = _client.CurrentRoom;
                    if (!string.IsNullOrEmpty(room))
                        _client.SendRoomForward(room, message.MessageId);
                    else
                        _client.SendForward(message.MessageId);
                }
            };
            menu.Items.Add(forwardItem);

            menu.Items.Add(new ToolStripSeparator());

            string[] emojis = { "😀", "😂", "😍", "👍", "❤️", "🎉" };
            foreach (string emoji in emojis)
            {
                var emojiItem = new ToolStripMenuItem($"{emoji} React");
                emojiItem.Click += (s, e) =>
                {
                    string room = _client.CurrentRoom;
                    if (!string.IsNullOrEmpty(room))
                        _client.SendRoomEmojiReaction(room, message.MessageId, emoji);
                    else
                        _client.SendEmojiReaction(message.MessageId, emoji);
                };
                menu.Items.Add(emojiItem);
            }

            menu.Show(txtChatContent, location);
        }

        private ChatLineEntry FindLineAt(int charIndex)
        {
            if (charIndex < 0) return null;

            foreach (var entry in _chatLines)
            {
                if (charIndex >= entry.StartIndex && charIndex < entry.StartIndex + entry.Length)
                    return entry;
            }
            return null;
        }

        private void SelectMessage(ChatLineEntry entry)
        {
            _selectedMessage = entry.Message;

            // Highlight nhẹ dòng đang chọn để người dùng biết đã chọn đúng message
            txtChatContent.Select(entry.StartIndex, entry.Length);
            txtChatContent.SelectionBackColor = Color.LightYellow;

            lblSelectedMessage.Text = $"Đã chọn: {entry.Message.Sender}: {Truncate(entry.Message.Content, 50)}";
            lblSelectedMessage.Visible = true;
        }

        // ===== Hiển thị message lên Chat Panel, đồng thời lưu vị trí để tra ngược khi click =====

        private void AppendChatMessage(ChatMessageInfo info)
        {
            string line = info.ToDisplayText();
            int start = txtChatContent.TextLength;

            txtChatContent.AppendText(line + Environment.NewLine);

            _chatLines.Add(new ChatLineEntry
            {
                StartIndex = start,
                Length = line.Length,
                Message = info
            });

            txtChatContent.SelectionStart = txtChatContent.TextLength;
            txtChatContent.ScrollToCaret();
        }

        private void AppendPlainLine(string text)
        {
            txtChatContent.AppendText(text + Environment.NewLine);
            txtChatContent.SelectionStart = txtChatContent.TextLength;
            txtChatContent.ScrollToCaret();
        }
        private void UpdateStatus(string s)
        {
            lblStatus.Text = s;
        }

        // ===== Giai đoạn 3: Room Panel =====

        private void RenderRoomList(List<string> rooms)
        {
            if (rooms == null) return;

            string previouslySelected = lstRooms.SelectedItem as string;

            lstRooms.Items.Clear();
            foreach (var room in rooms)
                lstRooms.Items.Add(room);

            HighlightCurrentRoomInList(_client.CurrentRoom ?? previouslySelected);
        }

        private void HighlightCurrentRoomInList(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return;

            for (int i = 0; i < lstRooms.Items.Count; i++)
            {
                if (string.Equals(lstRooms.Items[i] as string, roomName, StringComparison.OrdinalIgnoreCase))
                {
                    lstRooms.SelectedIndex = i;
                    return;
                }
            }
        }

        private void lstRooms_DoubleClick(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem is string roomName && !string.IsNullOrWhiteSpace(roomName))
            {
                if (!_client.IsConnected || !_client.IsLoggedIn)
                {
                    MessageBox.Show("Chưa kết nối hoặc chưa đăng nhập.");
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
                var label = new Label { Left = 16, Top = 16, Width = 320, Text = "Tên phòng mới:" };
                var textBox = new TextBox { Left = 16, Top = 40, Width = 310 };
                var okButton = new Button { Text = "Tạo", Left = 160, Top = 75, Width = 80, DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Hủy", Left = 246, Top = 75, Width = 80, DialogResult = DialogResult.Cancel };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(okButton);
                form.Controls.Add(cancelButton);
                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }

        private static Image TryDecodeAvatar(string avatarBase64)
        {
            if (string.IsNullOrWhiteSpace(avatarBase64))
                return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(avatarBase64);
                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        // ===== Helpers =====

        private static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= max ? text : text.Substring(0, max) + "...";
        }

        private static string ShortId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            return id.Length <= 8 ? id : id.Substring(0, 8);
        }

        private void SafeInvoke(Action action)
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