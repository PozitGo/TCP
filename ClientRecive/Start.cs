using System;
using System.Net;
using System.Threading.Tasks;
using ClientRecive.assets;

namespace ClientRecive
{
    internal class Start
    {
        public static Client client;
        static async Task Main(string[] args)
        {
            client = new Client(IPAddress.Parse("192.168.0.136"), 5345);
            await client.Connect();
        }
    }
}
