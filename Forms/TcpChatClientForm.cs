using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        // Message hiện đang được chọn để Reply / Forward (qua right-click hoặc double-click dòng chat)
        private ChatMessageInfo _selectedMessage;
        private ChatMessageInfo _pendingReply;

        // Lưu lại từng dòng chat đã hiển thị, map theo vị trí ký tự trong RichTextBox để xác định
        // người dùng đang right-click/double-click vào message nào.
        private readonly List<ChatLineEntry> _chatLines = new List<ChatLineEntry>();

        private class ChatLineEntry
        {
            public int StartIndex;
            public int Length;
            public ChatMessageInfo Message;
        }

        public TcpChatClientForm()
        {
            InitializeComponent();
            WireClientEvents();
            WireChatContextMenu();
            lstRooms.DoubleClick += lstRooms_DoubleClick;
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
                        _client.GetRooms();
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

            _client.OnChatMessage += info =>
                SafeInvoke(() =>
                {
                    // Message không-theo-room (2.5, RoomName rỗng) luôn hiển thị.
                    // Message theo-room (Giai đoạn 3) chỉ hiển thị nếu đúng room đang xem,
                    // tránh nhiễu nội dung giữa các phòng khác nhau.
                    if (string.IsNullOrEmpty(info.RoomName) ||
                        info.RoomName.Equals(_client.CurrentRoom, StringComparison.OrdinalIgnoreCase))
                    {
                        AppendChatMessage(info);
                    }
                });

            _client.OnSystemMessage += msg =>
                SafeInvoke(() =>
                {
                    AppendPlainLine("[System] " + msg);
                });

            _client.OnEmojiReaction += (messageId, reactor, emoji) =>
                SafeInvoke(() =>
                {
                    AppendPlainLine($"[Reaction] {reactor} đã react {emoji} vào tin nhắn {ShortId(messageId)}");
                });

            _client.OnDisconnected += () =>
                SafeInvoke(() =>
                {
                    lblStatus.Text = "Disconnected";
                    SetConnectionInputsEnabled(true);
                    SetChatControlsEnabled(false);
                });

            _client.OnUserListUpdated += users =>
                SafeInvoke(() => RenderUserList(users));

            // ===== Giai đoạn 3: Multi-Room =====

            _client.OnRoomListUpdated += rooms =>
                SafeInvoke(() => RenderRoomList(rooms));

            _client.OnJoinRoomOk += roomName =>
                SafeInvoke(() =>
                {
                    lblCurrentRoom.Text = $"Phòng: {roomName}";
                    HighlightCurrentRoomInList(roomName);
                    txtChatContent.Clear();
                    _chatLines.Clear();
                    AppendPlainLine($"--- Đã vào phòng '{roomName}' ---");
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

        private void WireChatContextMenu()
        {
            txtChatContent.MouseDown += TxtChatContent_MouseDown;
            txtChatContent.MouseDoubleClick += TxtChatContent_MouseDoubleClick;
        }

        private void TxtChatContent_MouseDoubleClick(object sender, MouseEventArgs e)
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

        // ===== Online User Panel với avatar nhỏ cạnh từng username =====

        private void RenderUserList(List<OnlineUserInfo> users)
        {
            pnlUsers.SuspendLayout();
            pnlUsers.Controls.Clear();

            int y = 4;
            foreach (var user in users)
            {
                var row = new Panel
                {
                    Location = new Point(4, y),
                    Size = new Size(pnlUsers.Width - 12, 36),
                    BackColor = Color.Transparent
                };

                var pic = new PictureBox
                {
                    Size = new Size(28, 28),
                    Location = new Point(0, 4),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle
                };

                Image avatarImg = TryDecodeAvatar(user.AvatarBase64);
                pic.Image = avatarImg; // null nếu không có avatar -> ô trống, không crash

                var lbl = new Label
                {
                    Text = user.Username,
                    Location = new Point(36, 8),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5F)
                };

                row.Controls.Add(pic);
                row.Controls.Add(lbl);
                pnlUsers.Controls.Add(row);

                y += 40;
            }

            pnlUsers.ResumeLayout();
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

                _client.JoinRoom(roomName);
            }
        }

        private void btnCreateRoom_Click(object sender, EventArgs e)
        {
            if (!_client.IsConnected || !_client.IsLoggedIn)
            {
                MessageBox.Show("Chưa kết nối hoặc chưa đăng nhập.");
                return;
            }

            string roomName = PromptForRoomName();
            if (string.IsNullOrWhiteSpace(roomName))
                return;

            _client.CreateRoom(roomName.Trim());
        }

        /// <summary>Hộp thoại nhập tên room tự viết (tránh phụ thuộc Microsoft.VisualBasic.Interaction).</summary>
        private static string PromptForRoomName()
        {
            using (var form = new Form
            {
                Width = 360,
                Height = 150,
                Text = "Tạo phòng chat",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
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
            if (IsDisposed) return;

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
    }
}