using System;
using System.Net;

namespace PublicClient
{
    public class Start
    {
        static void Main(string[] args)
        {
            Client client = new Client(IPAddress.Parse("109.123.252.229"), 5345);
            client.Connect();
        }
    }
}
