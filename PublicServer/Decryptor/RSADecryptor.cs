using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PublicServer.Decryptor
{
    public class RsaDecryptor
    {
        private readonly RSAParameters privateKey;

        public RsaDecryptor(RSAParameters privateKey)
        {
            this.privateKey = privateKey;
        }

        public string Decrypt(string data)
        {
            byte[] dataBytes = Convert.FromBase64String(data);
            byte[] decryptedBytes;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                decryptedBytes = rsa.Decrypt(dataBytes, true);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
