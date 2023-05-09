using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Control
{
    public class Server
    {
        private readonly IPAddress ServerIP;
        private readonly int ServerPort;
        private TcpListener server;
        public static Dictionary<IPAddress, TcpClient> clientList = new Dictionary<IPAddress, TcpClient>();

        public Server(string ServerIP, int ServerPort)
        {
            this.ServerIP = IPAddress.Parse(ServerIP);
            this.ServerPort = ServerPort;
        }
        public async Task Start()
        {
            server = new TcpListener(ServerIP, ServerPort);
            server.Start();

            Console.WriteLine("Сервер запущен...\nОжидает подключения клиента.");

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключился в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                    Console.ResetColor();
                    clientList.Add(IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()), client);
                    _ = Task.Factory.StartNew(() => CheckConnection(client));
                }
            });

            await Task.Factory.StartNew(async () => await HandleConnection()); 
        }

        public async Task HandleConnection()
        {
            try
            {
                string ipString = default;
                TcpClient client = default;

                while (true)
                {
                    if(clientList.Count != 0)
                    {
                        do
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("Введите IP адрес клиента для работы с ним:");
                            Console.ResetColor();

                            ipString = Console.ReadLine();

                            if (!IPAddress.TryParse(ipString, out IPAddress ipAddress))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Некорректный IP-адрес: {ipString}. Попробуйте еще раз.");
                                Console.ResetColor();
                                continue;
                            }

                            if (!clientList.TryGetValue(ipAddress, out client))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Клиент с IP-адресом {ipString} не подключен. Попробуйте еще раз.");
                                Console.ResetColor();
                                continue;
                            }

                            break;

                        } while (client is null);


                        ClientHandler clientHandler = new ClientHandler(client);
                        await clientHandler.Handle();
                    }

                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static void CheckConnection(TcpClient client)
        {
            while (true)
            {
                Thread.Sleep(10000);

                try
                {
                    NetworkStream stream = client.GetStream();

                    Socket socket = client.Client;
                    bool isConnected = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

                    if (!isConnected)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                        Console.ResetColor();
                        clientList.Remove(IPAddress.Parse(client.Client.RemoteEndPoint.ToString()));
                        break;
                    }
                }
                catch (SocketException)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                    Console.ResetColor();
                    clientList.Remove(IPAddress.Parse(client.Client.RemoteEndPoint.ToString()));
                    break;
                }
            }
        }
    }
}
