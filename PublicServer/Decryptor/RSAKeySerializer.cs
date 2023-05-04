using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

public static class RSAKeySerializer
{
    public static string SerializeToString(RSAParameters rsaParameters)
    {
        string serializedData = JsonConvert.SerializeObject(rsaParameters);
        return serializedData;
    }

    public static RSAParameters DeserializeFromString(string serializedDataString)
    {
        RSAParameters rsaParameters = JsonConvert.DeserializeObject<RSAParameters>(serializedDataString);
        return rsaParameters;
    }
}
