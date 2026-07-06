using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
