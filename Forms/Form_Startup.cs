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
    public partial class Form_Startup : Form
    {
        public Form_Startup()
        {
            InitializeComponent();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Nếu chọn Server
            if (radServer.Checked)
            {
                TCP_Chat_Server serverForm = new TCP_Chat_Server();
                serverForm.Show();
            }
            else
            {
                // Nếu chọn Client
                TCP_Chat_Client clientForm = new TCP_Chat_Client();
                clientForm.Show();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
   
