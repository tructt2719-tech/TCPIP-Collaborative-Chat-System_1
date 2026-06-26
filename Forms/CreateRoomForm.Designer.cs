namespace TCPIP_Collaborative_Chat_System.Forms
{
    partial class CreateRoomForm
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
            this.lblRoomName = new System.Windows.Forms.Label();
            this.txtRoomName = new System.Windows.Forms.TextBox();
            this.numMaxUsers = new System.Windows.Forms.NumericUpDown();
            this.lblMaxUsers = new System.Windows.Forms.Label();
            this.Privacy = new System.Windows.Forms.Label();
            this.radPublic = new System.Windows.Forms.RadioButton();
            this.radPrivate = new System.Windows.Forms.RadioButton();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // RoomName
            // 
            this.lblRoomName.AutoSize = true;
            this.lblRoomName.Location = new System.Drawing.Point(12, 18);
            this.lblRoomName.Name = "RoomName";
            this.lblRoomName.Size = new System.Drawing.Size(87, 16);
            this.lblRoomName.TabIndex = 0;
            this.lblRoomName.Text = "Room Name:";
            this.lblRoomName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtRoomName
            // 
            this.txtRoomName.Location = new System.Drawing.Point(139, 15);
            this.txtRoomName.Name = "txtRoomName";
            this.txtRoomName.Size = new System.Drawing.Size(605, 22);
            this.txtRoomName.TabIndex = 1;
            // 
            // numMaxUsers
            // 
            this.numMaxUsers.Location = new System.Drawing.Point(139, 53);
            this.numMaxUsers.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numMaxUsers.Name = "numMaxUsers";
            this.numMaxUsers.Size = new System.Drawing.Size(124, 22);
            this.numMaxUsers.TabIndex = 2;
            this.numMaxUsers.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // MaxUsers
            // 
            this.lblMaxUsers.AutoSize = true;
            this.lblMaxUsers.Location = new System.Drawing.Point(12, 59);
            this.lblMaxUsers.Name = "MaxUsers";
            this.lblMaxUsers.Size = new System.Drawing.Size(74, 16);
            this.lblMaxUsers.TabIndex = 3;
            this.lblMaxUsers.Text = "Max Users:";
            // 
            // Privacy
            // 
            this.Privacy.AutoSize = true;
            this.Privacy.Location = new System.Drawing.Point(12, 95);
            this.Privacy.Name = "Privacy";
            this.Privacy.Size = new System.Drawing.Size(55, 16);
            this.Privacy.TabIndex = 4;
            this.Privacy.Text = "Privacy:";
            // 
            // radPublic
            // 
            this.radPublic.AutoSize = true;
            this.radPublic.Checked = true;
            this.radPublic.Location = new System.Drawing.Point(139, 107);
            this.radPublic.Name = "radPublic";
            this.radPublic.Size = new System.Drawing.Size(65, 20);
            this.radPublic.TabIndex = 5;
            this.radPublic.TabStop = true;
            this.radPublic.Text = "Public";
            this.radPublic.UseVisualStyleBackColor = true;
            this.radPublic.CheckedChanged += new System.EventHandler(this.radPublic_CheckedChanged);
            // 
            // radPrivate
            // 
            this.radPrivate.AutoSize = true;
            this.radPrivate.Location = new System.Drawing.Point(139, 145);
            this.radPrivate.Name = "radPrivate";
            this.radPrivate.Size = new System.Drawing.Size(70, 20);
            this.radPrivate.TabIndex = 6;
            this.radPrivate.Text = "Private";
            this.radPrivate.UseVisualStyleBackColor = true;
            this.radPrivate.CheckedChanged += new System.EventHandler(this.radPrivate_CheckedChanged);
            // 
            // Password
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(398, 95);
            this.lblPassword.Name = "Password";
            this.lblPassword.Size = new System.Drawing.Size(70, 16);
            this.lblPassword.TabIndex = 7;
            this.lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(487, 89);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(257, 22);
            this.txtPassword.TabIndex = 8;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(219, 283);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(122, 41);
            this.btnCreate.TabIndex = 9;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(453, 283);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(122, 41);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CreateRoomForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.radPrivate);
            this.Controls.Add(this.radPublic);
            this.Controls.Add(this.Privacy);
            this.Controls.Add(this.lblMaxUsers);
            this.Controls.Add(this.numMaxUsers);
            this.Controls.Add(this.txtRoomName);
            this.Controls.Add(this.lblRoomName);
            this.Name = "CreateRoomForm";
            this.Text = "CreateRoomForm";
            ((System.ComponentModel.ISupportInitialize)(this.numMaxUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRoomName;
        private System.Windows.Forms.TextBox txtRoomName;
        private System.Windows.Forms.NumericUpDown numMaxUsers;
        private System.Windows.Forms.Label lblMaxUsers;
        private System.Windows.Forms.Label Privacy;
        private System.Windows.Forms.RadioButton radPublic;
        private System.Windows.Forms.RadioButton radPrivate;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
    }
}