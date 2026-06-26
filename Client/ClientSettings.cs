using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace TCPIP_Collaborative_Chat_System.Client
{
    public class ClientSettings
    {
        public string Username { get; set; }
        public bool Remember { get; set; }
        public string ServerIP { get; set; }
        public int Port { get; set; }
        private static readonly string FileName = "settings.json";
        
        
    }
}
