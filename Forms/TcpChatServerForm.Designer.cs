namespace TCPIP_Collaborative_Chat_System
{
    partial class TcpChatServerForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.Label lblServerPort;
        private System.Windows.Forms.NumericUpDown numServerPort;
        private System.Windows.Forms.Button btnInitServer;

        private System.Windows.Forms.Label lblStatus;

        private System.Windows.Forms.Label lblClientText;
        private System.Windows.Forms.Label lblClientCount;

        private System.Windows.Forms.Label lblMessageText;
        private System.Windows.Forms.Label lblTotalMessages;

        private System.Windows.Forms.ListBox lstUsers;

        private System.Windows.Forms.RichTextBox txtChatContent;

        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSendMessage;

        protected override void Dispose(bool disposing)
        {
            if (disposing &&
                (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle =
                new System.Windows.Forms.Label();

            this.lblServerPort =
                new System.Windows.Forms.Label();

            this.numServerPort =
                new System.Windows.Forms.NumericUpDown();

            this.btnInitServer =
                new System.Windows.Forms.Button();

            this.lblStatus =
                new System.Windows.Forms.Label();

            this.lblClientText =
                new System.Windows.Forms.Label();

            this.lblClientCount =
                new System.Windows.Forms.Label();

            this.lblMessageText =
                new System.Windows.Forms.Label();

            this.lblTotalMessages =
                new System.Windows.Forms.Label();

            this.lstUsers =
                new System.Windows.Forms.ListBox();

            this.txtChatContent =
                new System.Windows.Forms.RichTextBox();

            this.txtMessage =
                new System.Windows.Forms.TextBox();

            this.btnSendMessage =
                new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)
                (this.numServerPort))
                .BeginInit();

            this.SuspendLayout();

            // TITLE

            this.lblTitle.Text =
                "TCP/IP CHAT SERVER DASHBOARD";

            this.lblTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    18F,
                    System.Drawing.FontStyle.Bold);

            this.lblTitle.ForeColor =
                System.Drawing.Color.DodgerBlue;

            this.lblTitle.Location =
                new System.Drawing.Point(0, 15);

            this.lblTitle.Size =
                new System.Drawing.Size(1200, 40);

            this.lblTitle.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            // PORT

            this.lblServerPort.AutoSize = true;

            this.lblServerPort.Location =
                new System.Drawing.Point(20, 80);

            this.lblServerPort.Text =
                "Server Port";

            // PORT NUMBER

            this.numServerPort.Location =
                new System.Drawing.Point(110, 78);

            this.numServerPort.Maximum =
                65535;

            this.numServerPort.Value =
                9000;

            // START BUTTON

            this.btnInitServer.Text =
                "START SERVER";

            this.btnInitServer.Location =
                new System.Drawing.Point(260, 75);

            this.btnInitServer.Size =
                new System.Drawing.Size(140, 32);

            this.btnInitServer.Click +=
                new System.EventHandler(
                    this.btnInitServer_Click);

            // STATUS

            this.lblStatus.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.lblStatus.Text =
                "Server Offline";

            this.lblStatus.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            this.lblStatus.Location =
                new System.Drawing.Point(20, 125);

            this.lblStatus.Size =
                new System.Drawing.Size(1140, 30);

            // CLIENT COUNT

            this.lblClientText.AutoSize = true;

            this.lblClientText.Text =
                "Connected Users:";

            this.lblClientText.Location =
                new System.Drawing.Point(20, 175);

            this.lblClientCount.AutoSize = true;

            this.lblClientCount.Text =
                "0";

            this.lblClientCount.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F,
                    System.Drawing.FontStyle.Bold);

            this.lblClientCount.Location =
                new System.Drawing.Point(145, 173);

            // MESSAGE COUNT

            this.lblMessageText.AutoSize = true;

            this.lblMessageText.Text =
                "Messages:";

            this.lblMessageText.Location =
                new System.Drawing.Point(250, 175);

            this.lblTotalMessages.AutoSize = true;

            this.lblTotalMessages.Text =
                "0";

            this.lblTotalMessages.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F,
                    System.Drawing.FontStyle.Bold);

            this.lblTotalMessages.Location =
                new System.Drawing.Point(330, 173);

            // USERS

            this.lstUsers.Location =
                new System.Drawing.Point(20, 210);

            this.lstUsers.Size =
                new System.Drawing.Size(240, 330);

            // CHAT LOG

            this.txtChatContent.Location =
                new System.Drawing.Point(280, 210);

            this.txtChatContent.Size =
                new System.Drawing.Size(880, 330);

            this.txtChatContent.ReadOnly =
                true;

            // MESSAGE BOX

            this.txtMessage.Location =
                new System.Drawing.Point(20, 570);

            this.txtMessage.Size =
                new System.Drawing.Size(980, 22);

            // SEND

            this.btnSendMessage.Text =
                "BROADCAST";

            this.btnSendMessage.Location =
                new System.Drawing.Point(1020, 565);

            this.btnSendMessage.Size =
                new System.Drawing.Size(140, 35);

            this.btnSendMessage.Click +=
                new System.EventHandler(
                    this.btnSendMessage_Click);

            // FORM

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.BackColor =
                System.Drawing.Color.White;

            this.ClientSize =
                new System.Drawing.Size(1200, 650);

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Text =
                "TCP/IP Chat Server Dashboard";

            // ADD

            this.Controls.Add(this.lblTitle);

            this.Controls.Add(this.lblServerPort);
            this.Controls.Add(this.numServerPort);
            this.Controls.Add(this.btnInitServer);

            this.Controls.Add(this.lblStatus);

            this.Controls.Add(this.lblClientText);
            this.Controls.Add(this.lblClientCount);

            this.Controls.Add(this.lblMessageText);
            this.Controls.Add(this.lblTotalMessages);

            this.Controls.Add(this.lstUsers);

            this.Controls.Add(this.txtChatContent);

            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSendMessage);

            ((System.ComponentModel.ISupportInitialize)
                (this.numServerPort))
                .EndInit();

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}