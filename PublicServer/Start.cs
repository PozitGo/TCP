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
            JsonEncryptionService encryptionService = new JsonEncryptionService(@"C:\Users\Exper\Desktop");
            encryptionService.EncryptJsonToFile("sdfsdfsdf");

            Server server = new Server("192.168.0.136", 5345, encryptionService);
            server.Start();
        }
    }
}
