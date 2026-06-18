using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIP_Collaborative_Chat_System.Models
{
    internal class PacketModel
    {
        public string Command { get; set; }
        public string[] Parts { get; set; }
        public bool IsValid { get; set; }
        public PacketModel(string command, string[] parts, bool isValid)
        {
            Command = command;
            Parts = parts;
            IsValid = isValid;
        }
        public string Get(int index)
        {
            if (Parts == null || index < 0 || index >= Parts.Length) return string.Empty;
            return Parts[index].Trim();
        }
        public static PacketModel Invalid => new PacketModel(string.Empty, new string[0], false);
        public string GetPart(int index)
        {
            return (Parts != null && index < Parts.Length)
                ? Parts[index]
                : string.Empty;
        }

        public override string ToString() => $"[Packet: {Command} | Valid={IsValid}]";
    }
}
