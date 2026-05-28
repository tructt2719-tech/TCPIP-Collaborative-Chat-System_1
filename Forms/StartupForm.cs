using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class StartupForm : Form
    {
        public StartupForm()
        {
            InitializeComponent();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Nếu chọn Server
            if (radServer.Checked)
            {
                TcpChatServerForm serverForm = new TcpChatServerForm();
                serverForm.Show();
            }
            else
            {
                // Nếu chọn Client
                TcpChatClientForm clientForm = new TcpChatClientForm();
                clientForm.Show();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StartupForm_Load(object sender, EventArgs e)
        {

        }
    }
}
   
