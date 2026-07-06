namespace TCPIP_Collaborative_Chat_System.Services.Security
{
    public static class SecurityConfig
    {
        // AES256 Key (32 byte)
        public static readonly byte[] Key = System.Text.Encoding.UTF8.GetBytes( "12345678901234561234567890123456");

        // AES IV (16 byte)
        public static readonly byte[] IV = System.Text.Encoding.UTF8.GetBytes( "1234567890123456");
    }
}
