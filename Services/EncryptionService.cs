using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TCPIP_Collaborative_Chat_System.Services
{
    public static class EncryptionService
    {
        // 256-bit Key and 128-bit IV
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("TCPIPChatSystemSharedKey2026July"); // Exactly 32 bytes
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("TCPIPChatSysIV16");                  // Exactly 16 bytes

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = Iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = Iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (MemoryStream ms = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("CORRUPTED_PACKET: Base64 decoding failed.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("DECRYPT_FAILED: Decryption failed (invalid key or corrupted data).", ex);
            }
        }
    }
}
