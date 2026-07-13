using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPIP_Collaborative_Chat_System.Forms
{
    public partial class CreateRoomForm : Form
    {
        public CreateRoomForm()
        {
            InitializeComponent();
            txtPassword.Enabled = false;
        }
        public string RoomName
        {
            get { return txtRoomName.Text.Trim(); }
        }

        public int MaxUsers
        {
            get { return (int)numMaxUsers.Value; }
        }

        public bool IsPrivate
        {
            get { return radPrivate.Checked; }
        }

        public string Password
        {
            get { return txtPassword.Text.Trim(); }
        }
        private void radPublic_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radPrivate_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.Enabled = radPrivate.Checked;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomName.Text))
            {
                MessageBox.Show("Nhập tên phòng");
                return;
            }

            if (radPrivate.Checked &&
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Nhập mật khẩu");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
