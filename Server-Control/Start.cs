using System;
using System.Net;
using System.Threading.Tasks;
using Server_Control.assets;

namespace Server_Control
{
    internal class Start
    {
        static async Task Main(string[] args)
        {
            Server server = new Server(IPAddress.Parse("109.123.252.229"), 5345);
            await server.Start();
        }
    }
}
