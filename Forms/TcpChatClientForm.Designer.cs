namespace TCPIP_Collaborative_Chat_System
{
    partial class TcpChatClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblServerIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.numServerPort = new System.Windows.Forms.NumericUpDown();
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.RichTextBox();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.txtChatContent = new System.Windows.Forms.RichTextBox();
            this.Username = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.grpRooms = new System.Windows.Forms.GroupBox();
            this.txtRoomName = new System.Windows.Forms.TextBox();
            this.lblRooms = new System.Windows.Forms.TextBox();
            this.lstRooms = new System.Windows.Forms.ListBox();
            this.btnCreateRoom = new System.Windows.Forms.Button();
            this.btnJoinRoom = new System.Windows.Forms.Button();
            this.btnLeaveRoom = new System.Windows.Forms.Button();
            this.btnDeleteRoom = new System.Windows.Forms.Button();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.flpEmoji = new System.Windows.Forms.FlowLayoutPanel();
            this.btnEmoji = new System.Windows.Forms.Button();
            this.picAvatar = new System.Windows.Forms.PictureBox();
            this.btnChangeAvatar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).BeginInit();
            this.grpRooms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // lblServerIP
            // 
            this.lblServerIP.AutoSize = true;
            this.lblServerIP.Location = new System.Drawing.Point(12, 19);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(62, 16);
            this.lblServerIP.TabIndex = 0;
            this.lblServerIP.Text = "Server IP";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(427, 19);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(34, 16);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port:";
            // 
            // numServerPort
            // 
            this.numServerPort.Location = new System.Drawing.Point(478, 17);
            this.numServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numServerPort.MaximumSize = new System.Drawing.Size(65535, 0);
            this.numServerPort.Name = "numServerPort";
            this.numServerPort.Size = new System.Drawing.Size(120, 22);
            this.numServerPort.TabIndex = 3;
            this.numServerPort.Value = new decimal(new int[] {
            12345,
            0,
            0,
            0});
            // 
            // txtServerIP
            // 
            this.txtServerIP.Location = new System.Drawing.Point(92, 16);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(100, 22);
            this.txtServerIP.TabIndex = 2;
            // 
            // btnConnect
            // 
            this.btnConnect.ForeColor = System.Drawing.Color.Black;
            this.btnConnect.Location = new System.Drawing.Point(649, 16);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(94, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Kết nối";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.LightYellow;
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.Location = new System.Drawing.Point(12, 62);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(749, 26);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Chưa kết nối";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(63, 521);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(291, 31);
            this.txtMessage.Size = new System.Drawing.Size(279, 31);
            this.txtMessage.TabIndex = 6;
            this.txtMessage.Text = "";
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.BackColor = System.Drawing.Color.White;
            this.btnSendMessage.Location = new System.Drawing.Point(318, 521);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(98, 31);
            this.btnSendMessage.TabIndex = 7;
            this.btnSendMessage.Text = "Gửi tin nhắn";
            this.btnSendMessage.UseVisualStyleBackColor = false;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // txtChatContent
            // 
            this.txtChatContent.Location = new System.Drawing.Point(12, 143);
            this.txtChatContent.Name = "txtChatContent";
            this.txtChatContent.Size = new System.Drawing.Size(434, 302);
            this.txtChatContent.TabIndex = 8;
            this.txtChatContent.Text = "";
            this.txtChatContent.TextChanged += new System.EventHandler(this.txtChatContent_TextChanged);
            // 
            // Username
            // 
            this.Username.AutoSize = true;
            this.Username.Location = new System.Drawing.Point(209, 19);
            this.Username.Name = "Username";
            this.Username.Size = new System.Drawing.Size(70, 16);
            this.Username.TabIndex = 9;
            this.Username.Text = "Username";
            this.Username.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(285, 16);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(100, 22);
            this.txtUsername.TabIndex = 10;
            // 
            // grpRooms
            // 
            this.grpRooms.Controls.Add(this.txtRoomName);
            this.grpRooms.Controls.Add(this.lblRooms);
            this.grpRooms.Controls.Add(this.lstRooms);
            this.grpRooms.Location = new System.Drawing.Point(478, 109);
            this.grpRooms.Name = "grpRooms";
            this.grpRooms.Size = new System.Drawing.Size(296, 386);
            this.grpRooms.TabIndex = 11;
            this.grpRooms.TabStop = false;
            // 
            // txtRoomName
            // 
            this.txtRoomName.Location = new System.Drawing.Point(18, 331);
            this.txtRoomName.Name = "txtRoomName";
            this.txtRoomName.Size = new System.Drawing.Size(247, 22);
            this.txtRoomName.TabIndex = 7;
            // 
            // lblRooms
            // 
            this.lblRooms.BackColor = System.Drawing.SystemColors.Info;
            this.lblRooms.Location = new System.Drawing.Point(96, 6);
            this.lblRooms.Name = "lblRooms";
            this.lblRooms.Size = new System.Drawing.Size(100, 22);
            this.lblRooms.TabIndex = 6;
            this.lblRooms.Text = "Rooms";
            this.lblRooms.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblRooms.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // lstRooms
            // 
            this.lstRooms.FormattingEnabled = true;
            this.lstRooms.ItemHeight = 16;
            this.lstRooms.Items.AddRange(new object[] {
            "Study",
            "Music",
            "Team",
            "Gaming",
            "Work"});
            this.lstRooms.Location = new System.Drawing.Point(0, 68);
            this.lstRooms.Name = "lstRooms";
            this.lstRooms.Size = new System.Drawing.Size(296, 244);
            this.lstRooms.TabIndex = 0;
            // 
            // btnCreateRoom
            // 
            this.btnCreateRoom.BackColor = System.Drawing.Color.White;
            this.btnCreateRoom.ForeColor = System.Drawing.Color.Black;
            this.btnCreateRoom.Location = new System.Drawing.Point(422, 521);
            this.btnCreateRoom.Name = "btnCreateRoom";
            this.btnCreateRoom.Size = new System.Drawing.Size(95, 31);
            this.btnCreateRoom.TabIndex = 6;
            this.btnCreateRoom.Text = "Create";
            this.btnCreateRoom.UseVisualStyleBackColor = false;
            this.btnCreateRoom.Click += new System.EventHandler(this.btnCreateRoom_Click);
            // 
            // btnJoinRoom
            // 
            this.btnJoinRoom.Location = new System.Drawing.Point(523, 521);
            this.btnJoinRoom.Name = "btnJoinRoom";
            this.btnJoinRoom.Size = new System.Drawing.Size(86, 31);
            this.btnJoinRoom.TabIndex = 12;
            this.btnJoinRoom.Text = "Join";
            this.btnJoinRoom.UseVisualStyleBackColor = true;
            this.btnJoinRoom.Click += new System.EventHandler(this.btnJoinRoom_Click);
            // 
            // btnLeaveRoom
            // 
            this.btnLeaveRoom.Location = new System.Drawing.Point(615, 521);
            this.btnLeaveRoom.Name = "btnLeaveRoom";
            this.btnLeaveRoom.Size = new System.Drawing.Size(85, 31);
            this.btnLeaveRoom.TabIndex = 13;
            this.btnLeaveRoom.Text = "Leave";
            this.btnLeaveRoom.UseVisualStyleBackColor = true;
            this.btnLeaveRoom.Click += new System.EventHandler(this.btnLeaveRoom_Click);
            // 
            // btnDeleteRoom
            // 
            this.btnDeleteRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDeleteRoom.FlatAppearance.BorderSize = 0;
            this.btnDeleteRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteRoom.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnDeleteRoom.ForeColor = System.Drawing.Color.White;
            this.btnDeleteRoom.Location = new System.Drawing.Point(706, 521);
            this.btnDeleteRoom.Name = "btnDeleteRoom";
            this.btnDeleteRoom.Size = new System.Drawing.Size(82, 31);
            this.btnDeleteRoom.TabIndex = 14;
            this.btnDeleteRoom.Text = "🗑 Xóa phòng";
            this.btnDeleteRoom.UseVisualStyleBackColor = false;
            this.btnDeleteRoom.Click += new System.EventHandler(this.btnDeleteRoom_Click);
            // 
            // btnSendFile
            // 
            this.btnSendFile.Location = new System.Drawing.Point(348, 473);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(98, 32);
            this.btnSendFile.TabIndex = 14;
            this.btnSendFile.Text = "Send File";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
            // 
            // flpEmoji
            // 
            this.flpEmoji.AutoSize = true;
            this.flpEmoji.Location = new System.Drawing.Point(12, 473);
            this.flpEmoji.Name = "flpEmoji";
            this.flpEmoji.Size = new System.Drawing.Size(330, 32);
            this.flpEmoji.TabIndex = 15;
            this.flpEmoji.Visible = false;
            this.flpEmoji.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // btnEmoji
            // 
            this.btnEmoji.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.btnEmoji.Location = new System.Drawing.Point(14, 521);
            this.btnEmoji.Name = "btnEmoji";
            this.btnEmoji.Size = new System.Drawing.Size(43, 31);
            this.btnEmoji.TabIndex = 16;
            this.btnEmoji.Text = "😊";
            this.btnEmoji.UseVisualStyleBackColor = true;
            this.btnEmoji.Click += new System.EventHandler(this.btnEmoji_Click);
            // 
            // picAvatar
            // 
            this.picAvatar.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picAvatar.Location = new System.Drawing.Point(129, 91);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(85, 46);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatar.TabIndex = 17;
            this.picAvatar.TabStop = false;
            // 
            // btnChangeAvatar
            // 
            this.btnChangeAvatar.Location = new System.Drawing.Point(11, 109);
            this.btnChangeAvatar.Name = "btnChangeAvatar";
            this.btnChangeAvatar.Size = new System.Drawing.Size(112, 28);
            this.btnChangeAvatar.TabIndex = 18;
            this.btnChangeAvatar.Text = "Change Avatar";
            this.btnChangeAvatar.UseVisualStyleBackColor = true;
            this.btnChangeAvatar.Click += new System.EventHandler(this.btnChangeAvatar_Click);
            // 
            // TcpChatClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 574);
            this.Controls.Add(this.btnChangeAvatar);
            this.Controls.Add(this.picAvatar);
            this.Controls.Add(this.btnEmoji);
            this.Controls.Add(this.flpEmoji);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.btnLeaveRoom);
            this.Controls.Add(this.btnJoinRoom);
            this.Controls.Add(this.btnCreateRoom);
            this.Controls.Add(this.btnDeleteRoom);
            this.Controls.Add(this.grpRooms);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.Username);
            this.Controls.Add(this.txtChatContent);
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.numServerPort);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lblServerIP);
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "TcpChatClientForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "TcpChatClientForm";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).EndInit();
            this.grpRooms.ResumeLayout(false);
            this.grpRooms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblServerIP;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown numServerPort;
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.RichTextBox txtMessage;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.RichTextBox txtChatContent;
        private System.Windows.Forms.Label Username;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.GroupBox grpRooms;
        public System.Windows.Forms.ListBox lstRooms;
        private System.Windows.Forms.Button btnCreateRoom;
        private System.Windows.Forms.Button btnJoinRoom;
        private System.Windows.Forms.Button btnLeaveRoom;
        private System.Windows.Forms.Button btnDeleteRoom;
        private System.Windows.Forms.TextBox lblRooms;
        private System.Windows.Forms.TextBox txtRoomName;
        private System.Windows.Forms.Button btnSendFile;
        private System.Windows.Forms.FlowLayoutPanel flpEmoji;
        private System.Windows.Forms.Button btnEmoji;
        private System.Windows.Forms.PictureBox picAvatar;
        private System.Windows.Forms.Button btnChangeAvatar;
    }
}