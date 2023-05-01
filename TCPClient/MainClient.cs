using System.Threading.Tasks;
using System;
using TcpFileTransfer;

class Program
{
    static void Main(string[] args)
    {
        var client = new TcpFileTransferClient("192.168.0.136", 5345);
        client.ReceiveFile(@"C:\Users\Exper\Desktop\update.txt", @"C:\Users\Exper\Desktop\sdfs\update.txt");
    }
}