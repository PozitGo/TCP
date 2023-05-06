using System;
using System.Net.NetworkInformation;
using System.Threading;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true) 
            {
                if(PingServer("109.123.252.229"))
                {
                    Console.WriteLine("Живой");
                }
                else
                {
                    Console.WriteLine("Не живой");
                }

                Thread.Sleep(5000);
            }
        }

        static bool PingServer(string serverIP)
        {
            Ping ping = new Ping();

            try
            {
                PingReply reply = ping.Send(serverIP);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
