namespace TCPIP_Collaborative_Chat_System.Models
{
    public class RoomItem
    {
        public string Name { get; set; }

        public bool IsPrivate { get; set; }

        public override string ToString()
        {
            return IsPrivate
                ? $"🔒 {Name}"
                : Name;
        }
    }
}