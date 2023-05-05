using System;
using System.Net;
using System.Threading.Tasks;

namespace PublicClient
{
    public class Start
    {
        static async Task Main(string[] args)
        {
            Client client = new Client(IPAddress.Parse("192.168.0.136"), 5345);
            await client.Connect();
        }
    }
}
