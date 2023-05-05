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
            JsonEncryptionService encryptionService = new JsonEncryptionService(@"C:\Users\Exper\Desktop");
            encryptionService.EncryptJsonToFile("sdfsdfsdf");

            Server server = new Server("192.168.0.136", 5345, encryptionService);
            await server.Start();
        }
    }
}
