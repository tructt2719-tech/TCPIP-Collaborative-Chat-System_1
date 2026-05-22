namespace TCPIP_Collaborative_Chat_System
{
    partial class TCP_Chat_Client
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
            this.ServerIP = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.Label();
            this.numServerPort = new System.Windows.Forms.NumericUpDown();
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lbTrangThai = new System.Windows.Forms.Label();
            this.txtThongDiep = new System.Windows.Forms.RichTextBox();
            this.GửiTinNhắn = new System.Windows.Forms.Button();
            this.txtNoiDungChat = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).BeginInit();
            this.SuspendLayout();
            // 
            // ServerIP
            // 
            this.ServerIP.AutoSize = true;
            this.ServerIP.Location = new System.Drawing.Point(24, 19);
            this.ServerIP.Name = "ServerIP";
            this.ServerIP.Size = new System.Drawing.Size(62, 16);
            this.ServerIP.TabIndex = 0;
            this.ServerIP.Text = "Server IP";
            // 
            // Port
            // 
            this.Port.AutoSize = true;
            this.Port.Location = new System.Drawing.Point(336, 19);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(31, 16);
            this.Port.TabIndex = 1;
            this.Port.Text = "Port";
            // 
            // numServerPort
            // 
            this.numServerPort.Location = new System.Drawing.Point(397, 17);
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
            this.txtServerIP.Location = new System.Drawing.Point(115, 16);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(100, 22);
            this.txtServerIP.TabIndex = 2;
            this.txtServerIP.Text = "127.0.0.1";
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
            // lbTrangThai
            // 
            this.lbTrangThai.BackColor = System.Drawing.Color.LightYellow;
            this.lbTrangThai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbTrangThai.Location = new System.Drawing.Point(12, 62);
            this.lbTrangThai.Name = "lbTrangThai";
            this.lbTrangThai.Size = new System.Drawing.Size(749, 26);
            this.lbTrangThai.TabIndex = 5;
            this.lbTrangThai.Text = "Chưa kết nối";
            this.lbTrangThai.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtThongDiep
            // 
            this.txtThongDiep.Location = new System.Drawing.Point(12, 526);
            this.txtThongDiep.Name = "txtThongDiep";
            this.txtThongDiep.Size = new System.Drawing.Size(599, 26);
            this.txtThongDiep.TabIndex = 6;
            this.txtThongDiep.Text = "";
            // 
            // GửiTinNhắn
            // 
            this.GửiTinNhắn.Location = new System.Drawing.Point(649, 521);
            this.GửiTinNhắn.Name = "GửiTinNhắn";
            this.GửiTinNhắn.Size = new System.Drawing.Size(98, 31);
            this.GửiTinNhắn.TabIndex = 7;
            this.GửiTinNhắn.Text = "Gửi tin nhắn";
            this.GửiTinNhắn.UseVisualStyleBackColor = true;
            this.GửiTinNhắn.Click += new System.EventHandler(this.GửiTinNhắn_Click);
            // 
            // txtNoiDungChat
            // 
            this.txtNoiDungChat.Location = new System.Drawing.Point(12, 114);
            this.txtNoiDungChat.Name = "txtNoiDungChat";
            this.txtNoiDungChat.Size = new System.Drawing.Size(749, 386);
            this.txtNoiDungChat.TabIndex = 8;
            this.txtNoiDungChat.Text = "";
            // 
            // TCP_Chat_Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 574);
            this.Controls.Add(this.txtNoiDungChat);
            this.Controls.Add(this.GửiTinNhắn);
            this.Controls.Add(this.txtThongDiep);
            this.Controls.Add(this.lbTrangThai);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.numServerPort);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.ServerIP);
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "TCP_Chat_Client";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "TCP_Chat_Client";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ServerIP;
        private System.Windows.Forms.Label Port;
        private System.Windows.Forms.NumericUpDown numServerPort;
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lbTrangThai;
        private System.Windows.Forms.RichTextBox txtThongDiep;
        private System.Windows.Forms.Button GửiTinNhắn;
        private System.Windows.Forms.RichTextBox txtNoiDungChat;
    }
}