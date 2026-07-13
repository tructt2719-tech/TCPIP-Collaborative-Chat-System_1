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

        private readonly System.Collections.Generic.List<ChatMessage> _localMessageHistory = new System.Collections.Generic.List<ChatMessage>();
        private readonly System.Collections.Generic.List<DisplayedMessageInfo> _displayedMessages = new System.Collections.Generic.List<DisplayedMessageInfo>();
        private System.Collections.Generic.List<string> _onlineUsers = new System.Collections.Generic.List<string>();

        private Panel pnlReplyHeader;
        private Label lblReplyText;
        private Button btnCancelReply;
        private ChatMessage _currentReplyMessage = null;
        private ChatMessage _selectedMessageForContext = null;
        private ContextMenuStrip chatContextMenu;

        public TcpChatClientForm(string username, string password, bool remember)
        {
            _username = username;
            _password = password;
            _remember = remember;
            InitializeComponent();
            CreateEmojiButtons();
            LoadAvatar();
            txtUsername.Text = username;
            txtUsername.Enabled = false;
            WireClientEvents();
            LoadConnectionSettings();
            InitializeReplyUI();
            InitializeContextMenu();
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
                btn.Width = 36;
                btn.Height = 36;
                btn.Text = emoji;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font("Segoe UI Emoji", 14F);
                btn.Cursor = Cursors.Hand;
                btn.Click += (s, ev) =>
                {
                    int selStart = txtMessage.SelectionStart;
                    txtMessage.Text = txtMessage.Text.Insert(selStart, emoji);
                    txtMessage.SelectionStart = selStart + emoji.Length;
                    txtMessage.Focus();
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

            if (File.Exists(fullPath))
            {
                picAvatar.Image = Image.FromFile(fullPath);
            }
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
                    btnDisconnect.Enabled = false;
                }
                if (message == "Kết nối thành công.")
                {
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
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
                btnDisconnect.Enabled = false;
                _currentRoom = "";
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

            _client.OnUserListUpdated += users => SafeInvoke(() => {
                _onlineUsers = users;
                UpdateStatus("Online: " + string.Join(", ", users));
            });

            _client.OnRoomMsgReceived += msg => SafeInvoke(() => {
                _localMessageHistory.Add(msg);
                AppendMessageToUI(msg);
            });

            _client.OnRoomHistoryReceived += msg => SafeInvoke(() => {
                _localMessageHistory.Add(msg);
                AppendMessageToUI(msg);
            });

            _client.OnReplyMsgReceived += msg => SafeInvoke(() => {
                _localMessageHistory.Add(msg);
                AppendMessageToUI(msg);
            });

            _client.OnForwardMsgReceived += msg => SafeInvoke(() => {
                _localMessageHistory.Add(msg);
                AppendMessageToUI(msg);
            });

            _client.OnForwardPrivateReceived += msg => SafeInvoke(() => {
                _localMessageHistory.Add(msg);
                AppendMessageToUI(msg);
            });

            _client.OnDeleteMsgReceived += (msgId, roomName) => SafeInvoke(() => {
                var msg = _localMessageHistory.Find(m => m.MessageId == msgId);
                if (msg != null)
                {
                    msg.Content = "[Tin nhắn đã bị xóa]";
                    RenderChatHistory();
                }
            });

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
                if (_currentReplyMessage != null)
                {
                    _client.SendReply(_currentRoom, _currentReplyMessage.MessageId, txtMessage.Text.Trim());
                    CancelReply();
                }
                else
                {
                    _client.SendRoomMessage(_currentRoom, txtMessage.Text.Trim());
                }
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

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (!_client.IsConnected)
            {
                MessageBox.Show("Chưa kết nối đến Server!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Bạn có chắc muốn ngắt kết nối không?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _client.Disconnect();
            }
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null)
            {
                MessageBox.Show("Chọn phòng trước");
                return;
            }

            RoomItem room = GetSelectedRoom();
            if (room == null)
            {
                MessageBox.Show("Không thể xác định phòng được chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
            _localMessageHistory.Clear();
            _displayedMessages.Clear();
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

            RoomItem room = GetSelectedRoom();
            if (room == null)
            {
                MessageBox.Show("Không thể xác định phòng được chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn rời phòng '{room.Name}' ?", "Xác nhận rời phòng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _client.LeaveRoom(room.Name);
                _currentRoom = "";
            }
        }
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (!_client.IsConnected)
            {
                MessageBox.Show("Chưa kết nối đến Server!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_currentRoom))
            {
                MessageBox.Show("Bạn chưa tham gia phòng nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Chọn file để gửi";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _client.SendFile(dlg.FileName, _currentRoom);
                    UpdateChatContent($"[File] Đang gửi file: {System.IO.Path.GetFileName(dlg.FileName)}...");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gửi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
        private void btnDeleteRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phòng muốn xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            RoomItem room = GetSelectedRoom();
            if (room == null)
            {
                MessageBox.Show("Không thể xác định phòng được chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var confirm = MessageBox.Show($"Bạn có muốn xóa phòng \"{room.Name}\" không?\n\n" +
                "Yes\n" + "No", "Xác nhận xóa phòng", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm == DialogResult.Yes)
                _client.DeleteRoom(room.Name);
        }

        /// <summary>Safely retrieves selected room from lstRooms, handling both RoomItem and string entries.</summary>
        private RoomItem GetSelectedRoom()
        {
            object selected = lstRooms.SelectedItem;
            if (selected is RoomItem ri)
                return ri;
            if (selected is string roomName)
                return new RoomItem { Name = roomName, IsPrivate = false };
            return null;
        }

        public class DisplayedMessageInfo
        {
            public ChatMessage Message { get; set; }
            public int StartCharIndex { get; set; }
            public int EndCharIndex { get; set; }
        }

        private void AppendMessageToUI(ChatMessage msg)
        {
            int start = txtChatContent.TextLength;

            if (msg.IsReply && msg.ReplyMessageId.HasValue)
            {
                var orig = _localMessageHistory.Find(m => m.MessageId == msg.ReplyMessageId.Value);
                string replyInfo = orig != null 
                    ? (orig.Content == "[Tin nhắn đã bị xóa]" ? "Original message unavailable" : $"{orig.Sender}: \"{orig.Content}\"")
                    : "Original message unavailable";
                    
                txtChatContent.SelectionColor = Color.Gray;
                txtChatContent.SelectionFont = new Font(txtChatContent.Font.FontFamily, 8.5F, FontStyle.Italic);
                txtChatContent.AppendText($"  ↪ {replyInfo}\n");
            }
            else if (msg.IsForward && msg.ForwardMessageId.HasValue)
            {
                txtChatContent.SelectionColor = Color.Gray;
                txtChatContent.SelectionFont = new Font(txtChatContent.Font.FontFamily, 8.5F, FontStyle.Italic);
                txtChatContent.AppendText($"  [Forwarded message]\n");
            }

            // Append main message header
            txtChatContent.SelectionColor = msg.Sender == _username ? Color.Blue : Color.DarkGreen;
            txtChatContent.SelectionFont = new Font(txtChatContent.Font, FontStyle.Bold);
            
            if (msg.RoomName == null) // DM Context
            {
                if (msg.Sender == _username)
                {
                    txtChatContent.AppendText($"[Private to {msg.RoomName}] {msg.Sender}: ");
                }
                else
                {
                    txtChatContent.AppendText($"[Private from {msg.Sender}]: ");
                }
            }
            else
            {
                txtChatContent.AppendText($"{msg.Sender}: ");
            }

            // Append content
            txtChatContent.SelectionColor = Color.Black;
            txtChatContent.SelectionFont = new Font(txtChatContent.Font, FontStyle.Regular);
            txtChatContent.AppendText(msg.Content + "\n");
            
            int end = txtChatContent.TextLength;
            _displayedMessages.Add(new DisplayedMessageInfo { Message = msg, StartCharIndex = start, EndCharIndex = end });
            
            txtChatContent.SelectionStart = txtChatContent.TextLength;
            txtChatContent.ScrollToCaret();
        }

        private void RenderChatHistory()
        {
            txtChatContent.Clear();
            _displayedMessages.Clear();
            foreach (var msg in _localMessageHistory)
            {
                AppendMessageToUI(msg);
            }
        }

        private void InitializeReplyUI()
        {
            pnlReplyHeader = new Panel
            {
                Size = new Size(txtMessage.Width, 24),
                Location = new Point(txtMessage.Left, txtMessage.Top - 26),
                BackColor = Color.FromArgb(240, 240, 240),
                Visible = false
            };

            lblReplyText = new Label
            {
                Text = "",
                AutoSize = false,
                Size = new Size(pnlReplyHeader.Width - 30, 20),
                Location = new Point(3, 2),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };

            btnCancelReply = new Button
            {
                Text = "✕",
                Size = new Size(20, 20),
                Location = new Point(pnlReplyHeader.Width - 24, 2),
                FlatStyle = FlatStyle.Flat
            };
            btnCancelReply.FlatAppearance.BorderSize = 0;
            btnCancelReply.Click += (s, e) => CancelReply();

            pnlReplyHeader.Controls.Add(lblReplyText);
            pnlReplyHeader.Controls.Add(btnCancelReply);
            this.Controls.Add(pnlReplyHeader);
            pnlReplyHeader.BringToFront();
        }

        private void CancelReply()
        {
            _currentReplyMessage = null;
            pnlReplyHeader.Visible = false;
            lblReplyText.Text = "";
        }

        private void InitializeContextMenu()
        {
            chatContextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuReply = new ToolStripMenuItem("Reply", null, MenuReply_Click);
            ToolStripMenuItem menuForward = new ToolStripMenuItem("Forward", null, MenuForward_Click);
            ToolStripMenuItem menuCopy = new ToolStripMenuItem("Copy", null, MenuCopy_Click);
            ToolStripMenuItem menuDelete = new ToolStripMenuItem("Delete", null, MenuDelete_Click);

            chatContextMenu.Items.AddRange(new ToolStripItem[] { menuReply, menuForward, menuCopy, menuDelete });
            txtChatContent.ContextMenuStrip = chatContextMenu;
            txtChatContent.MouseDown += TxtChatContent_MouseDown;
        }

        private void TxtChatContent_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int charIndex = txtChatContent.GetCharIndexFromPosition(e.Location);
                var dispMsg = _displayedMessages.Find(m => charIndex >= m.StartCharIndex && charIndex < m.EndCharIndex);
                if (dispMsg != null)
                {
                    _selectedMessageForContext = dispMsg.Message;
                    chatContextMenu.Items[3].Enabled = (_selectedMessageForContext.Sender == _username);
                }
                else
                {
                    _selectedMessageForContext = null;
                }
            }
        }

        private void MenuReply_Click(object sender, EventArgs e)
        {
            if (_selectedMessageForContext == null) return;
            _currentReplyMessage = _selectedMessageForContext;
            pnlReplyHeader.Visible = true;
            lblReplyText.Text = $"↪ Replying to {_currentReplyMessage.Sender}: \"{_currentReplyMessage.Content}\"";
            txtMessage.Focus();
        }

        private void MenuCopy_Click(object sender, EventArgs e)
        {
            if (_selectedMessageForContext == null) return;
            Clipboard.SetText(_selectedMessageForContext.Content);
        }

        private void MenuForward_Click(object sender, EventArgs e)
        {
            if (_selectedMessageForContext == null) return;
            
            System.Collections.Generic.List<string> rooms = new System.Collections.Generic.List<string>();
            foreach (var item in lstRooms.Items)
            {
                if (item is RoomItem roomItem)
                {
                    rooms.Add(roomItem.Name);
                }
                else if (item is string rStr)
                {
                    rooms.Add(rStr);
                }
            }

            System.Collections.Generic.List<string> otherUsers = new System.Collections.Generic.List<string>();
            if (_onlineUsers != null)
            {
                foreach (var u in _onlineUsers)
                {
                    if (!u.Equals(_username, StringComparison.OrdinalIgnoreCase))
                    {
                        otherUsers.Add(u);
                    }
                }
            }

            Forms.ForwardForm dlg = new Forms.ForwardForm(rooms, otherUsers);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.IsUserForward)
                {
                    _client.SendForwardPrivate(_selectedMessageForContext.MessageId, dlg.SelectedDestination);
                }
                else
                {
                    _client.SendForwardRoom(_selectedMessageForContext.MessageId, dlg.SelectedDestination);
                }
            }
        }

        private void MenuDelete_Click(object sender, EventArgs e)
        {
            if (_selectedMessageForContext == null) return;
            var confirm = MessageBox.Show("Bạn có muốn xóa tin nhắn này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                _client.SendDeleteMsg(_selectedMessageForContext.MessageId, _selectedMessageForContext.RoomName);
            }
        }
    }
}
