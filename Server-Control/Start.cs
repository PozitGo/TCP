using System;
using System.Threading.Tasks;
using Server_Control.assets;

namespace Server_Control
{
    internal class Start
    {
        static async Task Main(string[] args)
        {
            Server server = new Server("vmi1292345.contaboserver.net", 5345);
            await server.Start();
        }
    }
}
