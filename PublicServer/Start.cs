using PublicServer.JSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace PublicServer
{
    public class Start
    {
        static void Main(string[] args)
        {
            JsonEncryptionService encryptionService = new JsonEncryptionService(@"/root/Server");
            encryptionService.EncryptJsonToFile("sdfsdfsdf");

            Server server = new Server("109.123.252.229", 5345, encryptionService);
            server.Start();
        }
    }
}
