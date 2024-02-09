using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PublicServer.Decryptor
{
    public static class RsaKeyGenerator
    {
        public static (RSAParameters publicKey, RSAParameters privateKey) GenerateKeyPair()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                return (rsa.ExportParameters(false), rsa.ExportParameters(true));
            }
        }
    }
}
