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
            client = new Client(IPAddress.Parse("109.123.252.229"), 5345);
            await client.Connect();
        }
    }
}
