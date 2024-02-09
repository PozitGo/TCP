using Server_Control.assets.Commands;
using Server_Control.assets.Commands.LocalCommands;
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

        public Server(IPAddress ServerIP, int ServerPort)
        {
            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
        }

        public Server(string Domain, int Port)
        {
            try
            {
                this.ServerIP = Dns.GetHostAddresses(Domain)[0];
                this.ServerPort = Port;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка в домене");
                Console.ResetColor();
            }
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

                    int currentLineCursor = Console.CursorTop;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, currentLineCursor);

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\nВведите команду или UUID клиента для работы с ним:");
                    Console.ResetColor();

                    while (true)
                    {
                        string Value = Console.ReadLine();

                        if (Value.Any(c => c == ' ') || (!Value.Any(c => c == ' ') && !Value.Contains("$")))
                        {
                            if (clientList.Count != 0)
                            {
                                if (Server.clientList.TryGetValue(Value, out TcpClient client))
                                {
                                    await HandleClientFound(Value, client);
                                    break;
                                }
                                else if (Value.Split(' ').Length >= 3 || Value.Split(' ').Length <= 2)
                                {
                                    string[] Data = Value.Split(' ');

                                    if (Data.Length >= 3)
                                    {
                                        Data[2] = string.Join(" ", Data.Skip(2));
                                    }

                                    if (Server.clientList.TryGetValue(Data[0], out TcpClient clientTwo))
                                    {
                                        await HandleClientFound(Data[0], clientTwo, Data);
                                        break;
                                    }
                                    else if (Data[0].Contains("all"))
                                    {
                                        if (Data.Length > 2)
                                        {
                                            await HandleAllClients(Data[1], Data[2]);
                                            break;
                                        }
                                        else
                                        {
                                            await HandleAllClients(Data[1]);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\rКлиент с UUID {Value} не подключен либо IP некорректен, либо контекст команды неверен. Попробуйте еще раз.");
                                        Console.ResetColor();

                                        int currentLineCursorError = Console.CursorTop;
                                        Console.SetCursorPosition(0, Console.CursorTop - 2);
                                        Console.Write(new string(' ', Console.WindowWidth));
                                        Console.SetCursorPosition(0, currentLineCursorError - 2);

                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"\rКлиент с UUID {Value} не подключен либо IP некорректен, либо контекст команды неверен. Попробуйте еще раз.");
                                    Console.ResetColor();

                                    int currentLineCursorError = Console.CursorTop;
                                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                                    Console.Write(new string(' ', Console.WindowWidth));
                                    Console.SetCursorPosition(0, currentLineCursorError - 2);

                                    break;
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ещё не один клиент не подключен, введите команду после подключения клиента");
                                Console.ResetColor();

                                int currentLineCursorError = Console.CursorTop;
                                Console.SetCursorPosition(0, Console.CursorTop - 2);
                                Console.Write(new string(' ', Console.WindowWidth));
                                Console.SetCursorPosition(0, currentLineCursorError - 2);


                                break;
                            }
                        }
                        else
                        {
                            await HandleCommandNoArguments(Value);
                            break;
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

        async Task HandleClientFound(string uuid, TcpClient client, string[] data = null)
        {
            Results.Success();

            if (client.Connected)
            {
                ClientHandler clientHandler;

                if (data != null && data.Length > 2)
                {
                    clientHandler = new ClientHandler(client, data[0], data[1], data[2]);
                }
                else if(data != null && data.Length <= 2)
                {
                    clientHandler = new ClientHandler(client, uuid, data[1]);
                }
                else
                {
                    clientHandler = new ClientHandler(client, uuid);
                }

                await clientHandler.Handle(data != null ? Usage.Reusable : Usage.Disposable);
            }
        }

        async Task HandleAllClients(string action, string param = null)
        {
            Results.Success();

            ClientHandler clientHandler = new ClientHandler(action, param);
            await clientHandler.Handle(Usage.All);
        }

        async Task HandleCommandNoArguments(string command)
        {
            switch (command)
            {
                case "$stop":

                    Task.Factory.StartNew(async () =>
                    {
                        ClientHandler clientHandler = new ClientHandler(command);
                        await clientHandler.Handle(Usage.All);
                    }).Wait();

                    Environment.Exit(0);

                    break;

                default:

                    await Task.Factory.StartNew(() =>
                    {
                        LocalCommandHandler localCommandHandler = new LocalCommandHandler(command);
                        localCommandHandler.LocalHandle();
                    });

                    break;
            }
        }
    }
}
