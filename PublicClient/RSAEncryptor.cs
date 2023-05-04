using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PublicClient
{
    public class RsaEncryptor
    {
        private readonly RSAParameters publicKey;

        public RsaEncryptor(RSAParameters publicKey)
        {
            this.publicKey = publicKey;
        }

        public string Encrypt(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                encryptedBytes = rsa.Encrypt(dataBytes, true);
            }

            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
