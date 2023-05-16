using Server_Control.assets.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Control.assets
{
    public class Server
    {
        private readonly IPAddress ServerIP;
        private readonly int ServerPort;
        private TcpListener server;
        public static readonly ConcurrentDictionary<string, TcpClient> clientList = new ConcurrentDictionary<string, TcpClient>();

        public Server(string ServerIP, int ServerPort)
        {
            this.ServerIP = IPAddress.Parse(ServerIP);
            this.ServerPort = ServerPort;
        }
        public Task Start()
        {
            server = new TcpListener(ServerIP, ServerPort);
            server.Start();

            Console.WriteLine("Сервер запущен...\nОжидает подключения клиента.");
            _ = Task.Factory.StartNew(() => HandleConnection());

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                string UUID = Guid.NewGuid().ToString().Substring(0, 5);

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключился c UUID {UUID} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();

                clientList.TryAdd(UUID, client);
                _ = Task.Factory.StartNew(() => CheckConnection(client, UUID));
            }
        }

        public async Task HandleConnection()
        {
            try
            {
                while (true)
                {
                    if (clientList.Count != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("\nВведите UUID клиента для работы с ним:");
                        Console.ResetColor();

                        while (true)
                        {
                            string UUID = Console.ReadLine();

                            if (clientList.TryGetValue(UUID, out TcpClient client) && client.Connected)
                            {

                                int currentLineCursor = Console.CursorTop;
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write(new string(' ', Console.WindowWidth));
                                Console.SetCursorPosition(0, currentLineCursor);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Успешно");
                                Console.ResetColor();

                                ClientHandler clientHandler = new ClientHandler(client, UUID);
                                await clientHandler.Handle();
                                break;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Клиент с UUID {UUID} не подключен либо IP некорректен. Попробуйте еще раз.");
                                Console.ResetColor();

                                int currentLineCursor = Console.CursorTop;
                                Console.SetCursorPosition(0, Console.CursorTop - 2);
                                Console.Write(new string(' ', Console.WindowWidth));
                                Console.SetCursorPosition(0, currentLineCursor - 2);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
                Console.ResetColor();
            }
        }



        public static void CheckConnection(TcpClient client, string UUID)
        {
            while (true)
            {
                Thread.Sleep(1000);

                try
                {
                    Socket socket = client.Client;
                    bool isConnected = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

                    if (!isConnected)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился c UUID {UUID} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                        Console.ResetColor();
                        clientList.TryRemove(UUID, out _);
                        break;
                    }
                }
                catch (SocketException)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился c UUID {UUID} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                    Console.ResetColor();
                    clientList.TryRemove(UUID, out _);
                    break;
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился c UUID {UUID} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                    Console.ResetColor();
                    clientList.TryRemove(UUID, out _);
                    break;
                }
            }
        }
    }
}
