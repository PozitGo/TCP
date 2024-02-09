using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PublicServer.JSON
{
    public class JsonEncryptionService
    {
        private readonly byte[] key;
        private readonly string JsonPath;

        public JsonEncryptionService(string filePath)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                key = new byte[32];
                rng.GetBytes(key);
            }

            this.JsonPath = filePath;
        }

        public void EncryptJsonToFile<T>(T obj)
        {
            var filePath = Path.Combine(JsonPath, "serverPass.json");

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            string jsonString = JsonConvert.SerializeObject(obj);
            byte[] encryptedBytes = EncryptStringToBytes(jsonString, key);

            File.WriteAllBytes(filePath, encryptedBytes);
        }


        public string DecryptJsonFromFile()
        {
            byte[] encryptedBytes = File.ReadAllBytes(Path.Combine(JsonPath, "serverPass.json"));
            string decryptedString = DecryptStringFromBytes(encryptedBytes, key);

            return JsonConvert.DeserializeObject<string>(decryptedString);
            Console.WriteLine("расшифровал жсон");
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] key)
        {
            byte[] iv = new byte[16];
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }

            return encrypted;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key)
        {
            byte[] iv = new byte[16];
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
