using System;
using System.Collections.Generic;
using System.Text;

namespace PublicServer
{
    public class Start
    {
        static void Main(string[] args)
        {
            Server server = new Server("192.168.0.136", 5345);
            server.Start();
        }
    }
}
