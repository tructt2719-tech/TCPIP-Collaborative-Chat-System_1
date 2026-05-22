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

namespace TCPIP_Collaborative_Chat_System
{
    public partial class TCP_Chat_Server : Form
    {
        public TCP_Chat_Server()
        {
            InitializeComponent();
        }
        //Khai bao 2 Socket
        Socket sckServer, sckClient;
        //sckServer: Cho ket noi den tu Client
        //sckClient: Truyen nhan du lieu voi Client
        private void KhoiTaoServer_Click(object sender, EventArgs e)
        {
            //Tao Socket
            sckServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Blind, listen
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, (int)numServerPort.Value);
            sckServer.Bind(ep);
            sckServer.Listen(5);
            //Accept
            //Ham Accept goi la ham Blocking, hay con goi la ham dong bo
            //Chuyen sang goi cac ham bat dong bo
            sckServer.BeginAccept(new AsyncCallback(XuLyKetNoi), null);
            //cap nhat trang thai dang ket noi
            CapNhatTrangThai("Dang cho ket noi...");
        }
        byte[] data = new byte[1024];
        private void XuLyKetNoi(IAsyncResult ar)
        {
            sckClient = sckServer.EndAccept(ar); //nhan ket qua tra ve la 1 socket moi
                                                 //cap nhat trang thai da ket noi
            lbTrangThai.Invoke(new CapnhatGUI(CapNhatTrangThai), new object[] { "Ket noi thanh cong" });
            //bat dau nhan du lieu
            sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(XuLyDuLieuNhanDuoc), null);
        }
        private void XuLyDuLieuNhanDuoc(IAsyncResult ar)
        {
            int size = sckClient.EndReceive(ar);
            //Xu ly du lieu trong buffer data
            string packet =
    Encoding.UTF8.GetString(data, 0, size);

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

        private void GuiTinNhan_Click(object sender, EventArgs e)
        {
            string packet =
                PacketBuilder.BuildMessage(
                    "Server",
                    txtThongDiep.Text);
            packet =  PacketBuilder.BuildMessage( "Server",txtThongDiep.Text);
            sckClient.Send(
                Encoding.UTF8.GetBytes(packet));
            CapNhatNoiDungChat("Server: " + txtThongDiep.Text);
            txtThongDiep.Text = ""; //Xoa noi dung cua textbox thong diep
            //luu y: do gui chuoi kha nhanh nen o day co the dung ham blocking Send()
            //Cac ban co the dung cap hang non-blocking BeginSend & EndSend tuong tu nhu BeginAccept
            
        }


        void CapNhatNoiDungChat(string s)
        {
            txtNoiDungChat.Text += s + "\r\n";
        }
    }
}
