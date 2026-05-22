using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPIP_Collaborative_Chat_System.Shared;

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TCP_Chat_Client : Form
    {
        public TCP_Chat_Client()
        {
            InitializeComponent();
        }

        Socket sckClient;
        private void btnConnect_Click(object sender, EventArgs e)
        {
            //Tao Socket
            sckClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Connect
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(txtServerIP.Text), (int)numServerPort.Value);
            sckClient.BeginConnect(ep, new AsyncCallback(XuLyKetNoi), null);
            //Goi ham bat dong bo
        }
        byte[] data = new byte[1024];
        private void XuLyKetNoi(IAsyncResult ar)
        {
            sckClient.EndConnect(ar);
            // Den day Client va Server da ket noi
            //Cap nhat trang thai va bat dau gui nhan du lieu
            lbTrangThai.Invoke(new CapnhatGUI(CapNhatTrangThai), new object[] { "Ket noi thanh cong." });
            //Bat dau nhan du lieu
            sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(XuLyDuLieuNhanDuoc), null);
        }
        private void XuLyDuLieuNhanDuoc(IAsyncResult ar)
        {
            int size = sckClient.EndReceive(ar);
            //Xu ly du lieu trong buffer data
            string packet = Encoding.UTF8.GetString(data, 0, size);
            packet = packet.Trim();
            string[] parts =
                PacketParser.Parse(packet);

            if (parts[0] == PacketTypes.MESSAGE)
            {
                string sender = parts[1];

                string message = parts[2];

                txtNoiDungChat.Invoke(
                    new CapnhatGUI(CapNhatNoiDungChat),
                    new object[]
                    {
            sender + ": " + message
                    });
            }
            //Tiep tuc cho nhan du lieu
            sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(XuLyDuLieuNhanDuoc), null);

        }
        delegate void CapnhatGUI(string s);
        void CapNhatTrangThai(string s)
        {
            lbTrangThai.Text = s;
        }

        void CapNhatNoiDungChat(string s)
        {
            txtNoiDungChat.Text += s + "\r\n";
        }

        private void GửiTinNhắn_Click(object sender, EventArgs e)
        {
            string packet =
                PacketBuilder.BuildMessage(
                    "Client",
                    txtThongDiep.Text);

            sckClient.Send(
                Encoding.UTF8.GetBytes(packet));

            CapNhatNoiDungChat(
                "Client: " + txtThongDiep.Text);

            txtThongDiep.Text = "";
        }
    }
}
