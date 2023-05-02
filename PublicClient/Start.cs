using System;
using System.Net;

namespace PublicClient
{
    public class Start
    {
        static void Main(string[] args)
        {
            Client client = new Client(IPAddress.Parse("192.168.0.136"), 5345);
            client.Connect();
        }
    }
}
