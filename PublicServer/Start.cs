using PublicServer.JSON;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicServer
{
    public class Start
    {
        static async Task Main(string[] args)
        {
            JsonEncryptionService encryptionService = new JsonEncryptionService(@"/root/Server");
            encryptionService.EncryptJsonToFile("sdfsdfsdf");

            Server server = new Server("109.123.252.229", 5345, encryptionService);
            await server.Start();
        }
    }
}
