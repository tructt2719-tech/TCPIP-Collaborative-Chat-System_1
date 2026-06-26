namespace TCPIP_Collaborative_Chat_System
{
    partial class StartupForm
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
            this.lblChooseMode = new System.Windows.Forms.Label();
            this.radClient = new System.Windows.Forms.RadioButton();
            this.radServer = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblChooseMode
            // 
            this.lblChooseMode.AutoSize = true;
            this.lblChooseMode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblChooseMode.Location = new System.Drawing.Point(12, 9);
            this.lblChooseMode.Name = "lblChooseMode";
            this.lblChooseMode.Size = new System.Drawing.Size(180, 28);
            this.lblChooseMode.TabIndex = 0;
            this.lblChooseMode.Text = "Chọn chế độ chạy";
            // 
            // radClient
            // 
            this.radClient.AutoSize = true;
            this.radClient.Font = new System.Drawing.Font("Segoe UI", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.radClient.Location = new System.Drawing.Point(224, 59);
            this.radClient.Name = "radClient";
            this.radClient.Size = new System.Drawing.Size(194, 27);
            this.radClient.TabIndex = 1;
            this.radClient.TabStop = true;
            this.radClient.Text = "Chạy ở chế độ Client";
            this.radClient.UseVisualStyleBackColor = true;
            // 
            // radServer
            // 
            this.radServer.AutoSize = true;
            this.radServer.Font = new System.Drawing.Font("Segoe UI", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.radServer.Location = new System.Drawing.Point(224, 111);
            this.radServer.Name = "radServer";
            this.radServer.Size = new System.Drawing.Size(198, 27);
            this.radServer.TabIndex = 2;
            this.radServer.TabStop = true;
            this.radServer.Text = "Chạy ở chế độ Server";
            this.radServer.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(203, 196);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 52);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExit.Location = new System.Drawing.Point(357, 196);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(84, 52);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Thoát";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.radServer);
            this.Controls.Add(this.radClient);
            this.Controls.Add(this.lblChooseMode);
            this.Name = "StartupForm";
            this.Text = "StartupForm";
            this.Load += new System.EventHandler(this.StartupForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblChooseMode;
        private System.Windows.Forms.RadioButton radClient;
        private System.Windows.Forms.RadioButton radServer;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnExit;
    }
}