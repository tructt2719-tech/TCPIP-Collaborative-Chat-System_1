using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TCPIP_Collaborative_Chat_System
{
    partial class TcpChatClientForm
    {
        private IContainer components = null;

        private Label lblTitle;

        private Label lblServerIP;
        private Label lblPort;
        private Label Username;
        private Label lblStatus;

        private TextBox txtServerIP;
        private TextBox txtUsername;

        private NumericUpDown numServerPort;

        private Button btnConnect;
        private Button btnSendMessage;

        private Button btnEmoji;
        private Button btnReply;
        private Button btnForward;
        private Button btnFile;
        private Button btnCancelReply;

        private RichTextBox txtChatContent;
        private RichTextBox txtMessage;

        private Label lblReplyPreview;
        private Label lblSelectedMessage;
        private Label lblUsersTitle;

        private Label lblRoomsTitle;
        private Label lblCurrentRoom;
        private Button btnCreateRoom;
        private ListBox lstRooms;

        /// <summary>
        /// Panel chứa danh sách Online User. Mỗi user được render thành 1 hàng con
        /// gồm PictureBox (avatar) + Label (username) - xem RenderUserList() trong TcpChatClientForm.cs.
        /// Thay cho ListBox cũ (lstUsers) vì ListBox không hỗ trợ hiển thị ảnh avatar.
        /// </summary>
        private Panel pnlUsers;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();

            this.lblServerIP = new Label();
            this.lblPort = new Label();
            this.Username = new Label();
            this.lblStatus = new Label();

            this.txtServerIP = new TextBox();
            this.txtUsername = new TextBox();

            this.numServerPort = new NumericUpDown();

            this.btnConnect = new Button();
            this.btnSendMessage = new Button();

            this.btnEmoji = new Button();
            this.btnReply = new Button();
            this.btnForward = new Button();
            this.btnFile = new Button();
            this.btnCancelReply = new Button();

            this.txtChatContent = new RichTextBox();
            this.txtMessage = new RichTextBox();

            this.lblReplyPreview = new Label();
            this.lblSelectedMessage = new Label();
            this.lblUsersTitle = new Label();

            this.lblRoomsTitle = new Label();
            this.lblCurrentRoom = new Label();
            this.btnCreateRoom = new Button();
            this.lstRooms = new ListBox();
            this.pnlUsers = new Panel();

            ((ISupportInitialize)(this.numServerPort)).BeginInit();

            this.SuspendLayout();

            // FORM

            this.BackColor = Color.White;
            this.ClientSize = new Size(1300, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "TCP/IP Collaborative Chat";

            // TITLE

            this.lblTitle.Text = "TCP/IP COLLABORATIVE CHAT SYSTEM";
            this.lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.DodgerBlue;
            this.lblTitle.Location = new Point(0, 10);
            this.lblTitle.Size = new Size(1300, 40);
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // SERVER IP

            this.lblServerIP.Text = "Server IP";
            this.lblServerIP.Location = new Point(20, 70);
            this.lblServerIP.AutoSize = true;

            this.txtServerIP.Location = new Point(90, 67);
            this.txtServerIP.Size = new Size(120, 22);
            this.txtServerIP.Text = "127.0.0.1";

            // USERNAME

            this.Username.Text = "Username";
            this.Username.Location = new Point(240, 70);
            this.Username.AutoSize = true;

            this.txtUsername.Location = new Point(320, 67);
            this.txtUsername.Size = new Size(150, 22);

            // PORT

            this.lblPort.Text = "Port";
            this.lblPort.Location = new Point(500, 70);
            this.lblPort.AutoSize = true;

            this.numServerPort.Location = new Point(540, 67);
            this.numServerPort.Maximum = 65535;
            this.numServerPort.Value = 9000;

            // CONNECT

            this.btnConnect.Text = "CONNECT";
            this.btnConnect.Location = new Point(700, 64);
            this.btnConnect.Size = new Size(140, 32);
            this.btnConnect.Click += new EventHandler(this.btnConnect_Click);

            // STATUS

            this.lblStatus.Text = "Disconnected";
            this.lblStatus.BorderStyle = BorderStyle.FixedSingle;
            this.lblStatus.Location = new Point(20, 110);
            this.lblStatus.Size = new Size(1250, 30);
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;

            // ROOMS

            this.lblRoomsTitle.Text = "Phòng Chat";
            this.lblRoomsTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.lblRoomsTitle.Location = new Point(20, 160);
            this.lblRoomsTitle.AutoSize = true;

            this.btnCreateRoom.Text = "+ Tạo phòng";
            this.btnCreateRoom.Location = new Point(140, 156);
            this.btnCreateRoom.Size = new Size(100, 26);
            this.btnCreateRoom.Click += new EventHandler(this.btnCreateRoom_Click);

            this.lstRooms.Location = new Point(20, 188);
            this.lstRooms.Size = new Size(220, 360);
            // Danh sách Room được Server đẩy về thật qua GET_ROOMS/ROOM_LIST/ROOM_CREATED,
            // xem RenderRoomList() trong TcpChatClientForm.cs. Double-click 1 room để JOIN_ROOM.

            this.lblCurrentRoom.Text = "Phòng: (chưa vào phòng nào)";
            this.lblCurrentRoom.ForeColor = Color.DodgerBlue;
            this.lblCurrentRoom.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblCurrentRoom.Location = new Point(20, 552);
            this.lblCurrentRoom.Size = new Size(220, 20);

            // CHAT

            this.txtChatContent.Location = new Point(260, 160);
            this.txtChatContent.Size = new Size(700, 420);
            this.txtChatContent.ReadOnly = true;
            this.txtChatContent.Font = new Font("Segoe UI", 10F);

            // USERS TITLE + PANEL (avatar + username mỗi hàng, xem RenderUserList)

            this.lblUsersTitle.Text = "Online Users";
            this.lblUsersTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.lblUsersTitle.Location = new Point(980, 160);
            this.lblUsersTitle.AutoSize = true;

            this.pnlUsers.Location = new Point(980, 184);
            this.pnlUsers.Size = new Size(290, 396);
            this.pnlUsers.BorderStyle = BorderStyle.FixedSingle;
            this.pnlUsers.AutoScroll = true;
            this.pnlUsers.BackColor = Color.White;

            // SELECTED MESSAGE indicator (cập nhật khi double-click / right-click 1 dòng chat)

            this.lblSelectedMessage.Text = "";
            this.lblSelectedMessage.ForeColor = Color.DimGray;
            this.lblSelectedMessage.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this.lblSelectedMessage.Location = new Point(260, 586);
            this.lblSelectedMessage.Size = new Size(700, 18);
            this.lblSelectedMessage.Visible = false;

            // REPLY PREVIEW BAR (hiện khi đang soạn reply, có nút huỷ)

            this.lblReplyPreview.Text = "";
            this.lblReplyPreview.ForeColor = Color.DodgerBlue;
            this.lblReplyPreview.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            this.lblReplyPreview.Location = new Point(20, 604);
            this.lblReplyPreview.Size = new Size(960, 18);
            this.lblReplyPreview.Visible = false;

            this.btnCancelReply.Text = "✕";
            this.btnCancelReply.Location = new Point(985, 600);
            this.btnCancelReply.Size = new Size(24, 24);
            this.btnCancelReply.Visible = true;
            this.btnCancelReply.Click += new EventHandler(this.btnCancelReply_Click);

            // MESSAGE

            this.txtMessage.Location = new Point(20, 630);
            this.txtMessage.Size = new Size(900, 40);
            this.txtMessage.KeyDown += new KeyEventHandler(this.txtMessage_KeyDown);

            // EMOJI

            this.btnEmoji.Text = "😀";
            this.btnEmoji.Location = new Point(940, 630);
            this.btnEmoji.Size = new Size(40, 40);
            this.btnEmoji.Click += new EventHandler(this.btnEmoji_Click);

            // REPLY

            this.btnReply.Text = "↩";
            this.btnReply.Location = new Point(990, 630);
            this.btnReply.Size = new Size(40, 40);
            this.btnReply.Click += new EventHandler(this.btnReply_Click);

            // FORWARD

            this.btnForward.Text = "➜";
            this.btnForward.Location = new Point(1040, 630);
            this.btnForward.Size = new Size(40, 40);
            this.btnForward.Click += new EventHandler(this.btnForward_Click);

            // FILE

            this.btnFile.Text = "📎";
            this.btnFile.Location = new Point(1090, 630);
            this.btnFile.Size = new Size(40, 40);
            this.btnFile.Click += new EventHandler(this.btnFile_Click);

            // SEND

            this.btnSendMessage.Text = "SEND";
            this.btnSendMessage.Location = new Point(1150, 630);
            this.btnSendMessage.Size = new Size(120, 40);
            this.btnSendMessage.Click += new EventHandler(this.btnSendMessage_Click);

            // CONTROLS

            this.Controls.Add(this.lblTitle);

            this.Controls.Add(this.lblServerIP);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.Username);

            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.txtUsername);

            this.Controls.Add(this.numServerPort);

            this.Controls.Add(this.btnConnect);

            this.Controls.Add(this.lblStatus);

            this.Controls.Add(this.lstRooms);
            this.Controls.Add(this.lblRoomsTitle);
            this.Controls.Add(this.btnCreateRoom);
            this.Controls.Add(this.lblCurrentRoom);

            this.Controls.Add(this.lblUsersTitle);
            this.Controls.Add(this.pnlUsers);

            this.Controls.Add(this.txtChatContent);
            this.Controls.Add(this.lblSelectedMessage);

            this.Controls.Add(this.lblReplyPreview);
            this.Controls.Add(this.btnCancelReply);

            this.Controls.Add(this.txtMessage);

            this.Controls.Add(this.btnEmoji);
            this.Controls.Add(this.btnReply);
            this.Controls.Add(this.btnForward);
            this.Controls.Add(this.btnFile);

            this.Controls.Add(this.btnSendMessage);

            ((ISupportInitialize)(this.numServerPort)).EndInit();

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}