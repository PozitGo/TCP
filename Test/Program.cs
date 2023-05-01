using System;
using System.Net.Sockets;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var port = 5345;
            var url = "pozitgo.tplinkdns.com";

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // пытаемся подключиться используя URL-адрес и порт
                socket.Connect(url, port);
                Console.WriteLine($"Подключение к {url} установлено");
            }
            catch (SocketException)
            {
                Console.WriteLine($"Не удалось установить подключение к {url}");
            }
        }
    }
}
