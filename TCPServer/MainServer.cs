using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TcpFileTransfer;

namespace TCPServer
{
    class MainServer
    {
        static void Main(string[] args)
        {
            var server = new TcpFileTransferServer(IPAddress.Parse("192.168.0.136"), 5345);
            server.Start();
        }
    }
}
