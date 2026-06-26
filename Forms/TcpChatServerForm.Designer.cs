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
            this.lblServerPort = new System.Windows.Forms.Label();
            this.lblServerIP = new System.Windows.Forms.Label();
            this.numServerPort = new System.Windows.Forms.NumericUpDown();
            this.btnInitServer = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.txtChatContent = new System.Windows.Forms.RichTextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).BeginInit();
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
            this.lblServerPort.Location = new System.Drawing.Point(33, 27);
            this.lblServerPort.Name = "lblServerPort";
            this.lblServerPort.Size = new System.Drawing.Size(34, 16);
            this.lblServerPort.TabIndex = 0;
            this.lblServerPort.Text = "Port:";
            // 
            // lblServerIP
            // 
            this.lblServerIP.AutoSize = true;
            this.lblServerIP.Location = new System.Drawing.Point(327, 27);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(77, 16);
            this.lblServerIP.TabIndex = 8;
            this.lblServerIP.Text = "IP : Ch?a có";
            // 
            // numServerPort
            // 
            this.numServerPort.Location = new System.Drawing.Point(98, 21);
            this.numServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numServerPort.Name = "numServerPort";
            this.numServerPort.Size = new System.Drawing.Size(120, 22);
            this.numServerPort.TabIndex = 1;
            this.numServerPort.Value = new decimal(new int[] {
            12345,
            0,
            0,
            0});
            // 
            // btnInitServer
            // 
            this.btnInitServer.Location = new System.Drawing.Point(651, 23);
            this.btnInitServer.Name = "btnInitServer";
            this.btnInitServer.Size = new System.Drawing.Size(119, 30);
            this.btnInitServer.TabIndex = 2;
            this.btnInitServer.Text = "Kh?i t?o Server";
            this.btnInitServer.UseVisualStyleBackColor = true;
            this.btnInitServer.Click += new System.EventHandler(this.btnInitServer_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.LightYellow;
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.Location = new System.Drawing.Point(15, 73);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(755, 23);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Ch?a k?t n?i";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Location = new System.Drawing.Point(651, 410);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(119, 23);
            this.btnSendMessage.TabIndex = 5;
            this.btnSendMessage.Text = "G?i tin nh?n";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // txtChatContent
            // 
            this.txtChatContent.Location = new System.Drawing.Point(15, 118);
            this.txtChatContent.Name = "txtChatContent";
            this.txtChatContent.Size = new System.Drawing.Size(755, 266);
            this.txtChatContent.TabIndex = 6;
            this.txtChatContent.Text = "";
            this.txtChatContent.TextChanged += new System.EventHandler(this.txtChatContent_TextChanged);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(15, 411);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(602, 22);
            this.txtMessage.TabIndex = 7;
            // 
            // TcpChatServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnInitServer);
            this.Controls.Add(this.numServerPort);
            this.Controls.Add(this.lblServerPort);
            this.Controls.Add(this.lblServerIP);
            this.Name = "TcpChatServerForm";
            this.Text = "TcpChatServerForm";
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblServerPort;
        private System.Windows.Forms.NumericUpDown numServerPort;
        private System.Windows.Forms.Button btnInitServer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.RichTextBox txtChatContent;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Label lblServerIP;
    }
}