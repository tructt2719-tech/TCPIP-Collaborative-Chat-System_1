using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Forms;
using TCPIP_Collaborative_Chat_System.Models;

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
            LoginForm login;

            if (radServer.Checked)
            {
                login = new LoginForm(AppMode.Server);
            }
            else
            {
                login = new LoginForm(AppMode.Client);
            }

            login.Show();
            
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
   
