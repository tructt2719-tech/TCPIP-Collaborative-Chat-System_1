using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TCPIP_Collaborative_Chat_System.Forms
{
    public class ForwardForm : Form
    {
        private ListBox lstDestinations;
        private Button btnForward;
        private Button btnCancel;
        private Label lblPrompt;

        public string SelectedDestination { get; private set; }
        public bool IsUserForward { get; private set; }

        public ForwardForm(List<string> rooms, List<string> onlineUsers)
        {
            InitializeComponent(rooms, onlineUsers);
        }

        private void InitializeComponent(List<string> rooms, List<string> onlineUsers)
        {
            this.Text = "Chuyển tiếp tin nhắn";
            this.Size = new Size(320, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            lblPrompt = new Label
            {
                Text = "Chọn phòng hoặc người nhận để chuyển tiếp:",
                Location = new Point(12, 12),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            lstDestinations = new ListBox
            {
                Location = new Point(12, 35),
                Size = new Size(280, 220),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            // Populate rooms
            foreach (var room in rooms)
            {
                lstDestinations.Items.Add($"[Room] {room}");
            }

            // Populate users
            foreach (var user in onlineUsers)
            {
                lstDestinations.Items.Add($"[User] {user}");
            }

            btnForward = new Button
            {
                Text = "Chuyển tiếp",
                Location = new Point(116, 280),
                Size = new Size(88, 28),
                DialogResult = DialogResult.OK
            };
            btnForward.Click += BtnForward_Click;

            btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(210, 280),
                Size = new Size(82, 28),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { lblPrompt, lstDestinations, btnForward, btnCancel });
            this.AcceptButton = btnForward;
            this.CancelButton = btnCancel;
        }

        private void BtnForward_Click(object sender, EventArgs e)
        {
            if (lstDestinations.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một điểm đến!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string selected = lstDestinations.SelectedItem.ToString();
            if (selected.StartsWith("[Room] "))
            {
                SelectedDestination = selected.Substring(7);
                IsUserForward = false;
            }
            else if (selected.StartsWith("[User] "))
            {
                SelectedDestination = selected.Substring(7);
                IsUserForward = true;
            }
        }
    }
}
