using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    public class MessageModel
    {
        // [Giai đoạn 2.5 - Bước 8] Người gửi (do Server gắn vào)
        public string Sender { get; set; } = string.Empty;

        // Nội dung tin nhắn
        public string Content { get; set; } = string.Empty;
    }
}
