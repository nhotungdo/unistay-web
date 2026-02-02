using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Unistay_Web.Helpers
{
    public static class MessageEncryptionHelper
    {
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("UnistayWebSecureMessagingKey2026!!");
        private static readonly byte[] EncryptionIV = Encoding.UTF8.GetBytes("UnistayWebIV2026");

        public static string Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.IV = EncryptionIV;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch { return plainText ?? string.Empty; }
        }

        public static string Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            try
            {
                var buffer = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.IV = EncryptionIV;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback logic
                try 
                {
                    var data = Convert.FromBase64String(cipherText);
                    return Encoding.UTF8.GetString(data);
                } 
                catch 
                { 
                    return cipherText; 
                }
            }
        }
    }
}
