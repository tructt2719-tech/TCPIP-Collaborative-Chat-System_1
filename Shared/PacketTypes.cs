using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketTypes
    {
        public const string Login = "LOGIN";

        public const string Message = "MESSAGE";

        public const string Disconnect = "DISCONNECT";
    }
}
<<<<<<< Updated upstream
=======
public static class PacketTypes
{
    public const string LOGIN = "LOGIN";

    public const string MESSAGE = "MESSAGE";

    public const string DISCONNECT = "DISCONNECT";
    //2.5 ── Message ──
    public const string MSG = "MSG";          // Client→Server: MSG|noi dung
                                              // Server→Client: MSG|sender|noi dung
}
>>>>>>> Stashed changes
