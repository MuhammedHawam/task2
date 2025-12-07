using System;
using System.IO;
using System.Security.Cryptography;

namespace PIF.EBP.Core.Helpers
{
    public static class CryptoUtils
    {
        public const string aesKey = "KBCKvcLslUuB4y3EBlKate7BGowtHski1LzyqJHvUhs=";
        public static Guid aesIV = new Guid("5536e317-5379-ed11-b4d2-0022480d5665");
        /// <summary>
        /// Encrypt the Plain Text with the AES Encryption Logic
        /// </summary>
        /// <param name="plainText">plain text</param>
        /// <param name="userNameIv">guid username</param>
        /// <returns>returns encrypt string</returns>
        public static string Encrypt(string plainText)
        {

            string stEncryptedText = string.Empty;

            if (!string.IsNullOrWhiteSpace(plainText))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.KeySize = 256;

                    aes.Key = Convert.FromBase64String(aesKey);
                    aes.IV = FromHex(BitConverter.ToString(aesIV.ToByteArray()).Replace("-", string.Empty));

                    ICryptoTransform encryptor = aes.CreateEncryptor(FromHex(BitConverter.ToString(aes.Key).Replace("-", string.Empty)), aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }
                        }

                        stEncryptedText = BitConverter.ToString(memoryStream.ToArray()).Replace("-", string.Empty);
                    }
                }
            }

            return stEncryptedText;

        }

        /// <summary>
        /// Decrypt the Encrypted Text using AES Decryption Logic
        /// </summary>
        /// <param name="strEncryptedText">cipher text</param>
        /// <param name="userNameIv">guid user</param>
        /// <returns>returns decrypt string</returns>
        public static string Decrypt(string strEncryptedText)
        {
            string strPlainText = string.Empty;

            if (!string.IsNullOrWhiteSpace(strEncryptedText))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.KeySize = 256;

                    aes.Key = Convert.FromBase64String(aesKey);
                    aes.IV = FromHex(BitConverter.ToString(aesIV.ToByteArray()).Replace("-", string.Empty));

                    ICryptoTransform decryptor = aes.CreateDecryptor(FromHex(BitConverter.ToString(aes.Key).Replace("-", string.Empty)), aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(FromHex(strEncryptedText)))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                strPlainText = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }


            return strPlainText;
        }

        /// <summary>
        /// convert byte from string
        /// </summary>
        /// <param name="hex">hex</param>
        /// <returns>byte from string</returns>
        private static byte[] FromHex(string hex)
        {
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}
