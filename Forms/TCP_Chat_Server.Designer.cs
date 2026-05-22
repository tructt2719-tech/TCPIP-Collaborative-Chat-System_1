namespace TCPIP_Collaborative_Chat_System
{
    partial class TCP_Chat_Server
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
            this.ServerChayTrenPort = new System.Windows.Forms.Label();
            this.numServerPort = new System.Windows.Forms.NumericUpDown();
            this.KhoiTaoServer = new System.Windows.Forms.Button();
            this.lbTrangThai = new System.Windows.Forms.Label();
            this.GuiTinNhan = new System.Windows.Forms.Button();
            this.txtNoiDungChat = new System.Windows.Forms.RichTextBox();
            this.txtThongDiep = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).BeginInit();
            this.SuspendLayout();
            // 
            // ServerChayTrenPort
            // 
            this.ServerChayTrenPort.AutoSize = true;
            this.ServerChayTrenPort.Location = new System.Drawing.Point(12, 23);
            this.ServerChayTrenPort.Name = "ServerChayTrenPort";
            this.ServerChayTrenPort.Size = new System.Drawing.Size(131, 16);
            this.ServerChayTrenPort.TabIndex = 0;
            this.ServerChayTrenPort.Text = "Server chạy trên Port";
            // 
            // numServerPort
            // 
            this.numServerPort.Location = new System.Drawing.Point(174, 23);
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
            // KhoiTaoServer
            // 
            this.KhoiTaoServer.Location = new System.Drawing.Point(651, 23);
            this.KhoiTaoServer.Name = "KhoiTaoServer";
            this.KhoiTaoServer.Size = new System.Drawing.Size(119, 30);
            this.KhoiTaoServer.TabIndex = 2;
            this.KhoiTaoServer.Text = "Khởi tạo Server";
            this.KhoiTaoServer.UseVisualStyleBackColor = true;
            this.KhoiTaoServer.Click += new System.EventHandler(this.KhoiTaoServer_Click);
            // 
            // lbTrangThai
            // 
            this.lbTrangThai.BackColor = System.Drawing.Color.LightYellow;
            this.lbTrangThai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbTrangThai.Location = new System.Drawing.Point(15, 73);
            this.lbTrangThai.Name = "lbTrangThai";
            this.lbTrangThai.Size = new System.Drawing.Size(755, 23);
            this.lbTrangThai.TabIndex = 3;
            this.lbTrangThai.Text = "Chưa kết nối";
            this.lbTrangThai.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GuiTinNhan
            // 
            this.GuiTinNhan.Location = new System.Drawing.Point(651, 410);
            this.GuiTinNhan.Name = "GuiTinNhan";
            this.GuiTinNhan.Size = new System.Drawing.Size(119, 23);
            this.GuiTinNhan.TabIndex = 5;
            this.GuiTinNhan.Text = "Gửi tin nhắn";
            this.GuiTinNhan.UseVisualStyleBackColor = true;
            this.GuiTinNhan.Click += new System.EventHandler(this.GuiTinNhan_Click);
            // 
            // txtNoiDungChat
            // 
            this.txtNoiDungChat.Location = new System.Drawing.Point(15, 118);
            this.txtNoiDungChat.Name = "txtNoiDungChat";
            this.txtNoiDungChat.Size = new System.Drawing.Size(755, 266);
            this.txtNoiDungChat.TabIndex = 6;
            this.txtNoiDungChat.Text = "";
            // 
            // txtThongDiep
            // 
            this.txtThongDiep.Location = new System.Drawing.Point(15, 411);
            this.txtThongDiep.Name = "txtThongDiep";
            this.txtThongDiep.Size = new System.Drawing.Size(602, 22);
            this.txtThongDiep.TabIndex = 7;
            // 
            // TCP_Chat_Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtThongDiep);
            this.Controls.Add(this.txtNoiDungChat);
            this.Controls.Add(this.GuiTinNhan);
            this.Controls.Add(this.lbTrangThai);
            this.Controls.Add(this.KhoiTaoServer);
            this.Controls.Add(this.numServerPort);
            this.Controls.Add(this.ServerChayTrenPort);
            this.Name = "TCP_Chat_Server";
            this.Text = "TCP_Chat_Server";
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ServerChayTrenPort;
        private System.Windows.Forms.NumericUpDown numServerPort;
        private System.Windows.Forms.Button KhoiTaoServer;
        private System.Windows.Forms.Label lbTrangThai;
        private System.Windows.Forms.Button GuiTinNhan;
        private System.Windows.Forms.RichTextBox txtNoiDungChat;
        private System.Windows.Forms.TextBox txtThongDiep;
    }
}