using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Shared
{
    internal class EncodingHelper
    {
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
        public static byte[] ToBytes(string packet)
        {
            return DefaultEncoding.GetBytes(packet + PacketTypes.NewLine);
        }
        public static string FromBytes(byte[] buffer, int count)
        {
            return DefaultEncoding.GetString(buffer, 0, count);
        }
    }
}
