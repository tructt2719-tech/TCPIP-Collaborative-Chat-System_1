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

        private RichTextBox txtChatContent;
        private RichTextBox txtMessage;

        private ListBox lstUsers;
        private ListBox lstRooms;

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

            this.txtChatContent = new RichTextBox();
            this.txtMessage = new RichTextBox();

            this.lstUsers = new ListBox();
            this.lstRooms = new ListBox();

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

            this.lstRooms.Location = new Point(20, 160);
            this.lstRooms.Size = new Size(220, 420);

            this.lstRooms.Items.Add("General");
            this.lstRooms.Items.Add("Study");
            this.lstRooms.Items.Add("Gaming");
            this.lstRooms.Items.Add("Team");

            // CHAT

            this.txtChatContent.Location = new Point(260, 160);
            this.txtChatContent.Size = new Size(760, 420);
            this.txtChatContent.ReadOnly = true;
            this.txtChatContent.Font = new Font("Segoe UI", 10F);

            // USERS

            this.lstUsers.Location = new Point(1040, 160);
            this.lstUsers.Size = new Size(230, 420);

            // MESSAGE

            this.txtMessage.Location = new Point(20, 620);
            this.txtMessage.Size = new Size(900, 40);

            // EMOJI

            this.btnEmoji.Text = "😀";
            this.btnEmoji.Location = new Point(940, 620);
            this.btnEmoji.Size = new Size(40, 40);

            // REPLY

            this.btnReply.Text = "↩";
            this.btnReply.Location = new Point(990, 620);
            this.btnReply.Size = new Size(40, 40);

            // FORWARD

            this.btnForward.Text = "➜";
            this.btnForward.Location = new Point(1040, 620);
            this.btnForward.Size = new Size(40, 40);

            // FILE

            this.btnFile.Text = "📎";
            this.btnFile.Location = new Point(1090, 620);
            this.btnFile.Size = new Size(40, 40);

            // SEND

            this.btnSendMessage.Text = "SEND";
            this.btnSendMessage.Location = new Point(1150, 620);
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
            this.Controls.Add(this.lstUsers);

            this.Controls.Add(this.txtChatContent);

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