using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TCPIP_Collaborative_Chat_System.Client
{
    public static class SettingsManager
    {
        private static readonly string FileName = "settings.ini";
        public static void Save(string username, bool remember, string serverIP, int port)
        {
            string[] data = {"Remember=" + remember, "Username=" + username, "ServerIP=" + serverIP, "Port=" + port};

            File.WriteAllLines(FileName, data);
        }
        public static bool Exists()
        {
            return File.Exists(FileName);
        }
        public static string Read(string key)
        {
            if (!Exists())
                return "";

            foreach (string line in File.ReadAllLines(FileName))
            {
                if (line.StartsWith(key + "="))
                {
                    return line.Substring(key.Length + 1);
                }
            }

            return "";
        }
        public static void Delete()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
    }
}