namespace TCPIP_Collaborative_Chat_System.Shared
{
    public static class PacketTypes
    {
        public const string Login = "LOGIN";
        public const string LoginOk = "LOGIN_OK";
        public const string LoginFail = "LOGIN_FAIL";
        public const string Register = "REGISTER";
        public const string RegisterOk = "REGISTER_OK";
        public const string RegisterFail = "REGISTER_FAIL";
        public const string Message = "MESSAGE";
        public const string Disconnect = "DISCONNECT";
        public const string System = "SYSTEM";
        public const string UserList = "USER_LIST";
    }
}