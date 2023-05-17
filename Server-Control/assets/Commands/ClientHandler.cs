using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Control.assets.Commands
{
    public class ClientHandler
    {
        private readonly TcpClient client;
        private string UUID;
        private string Command;
        private string Arguments;

        public ClientHandler(TcpClient client, string UUID)
        {
            this.UUID = UUID;
            this.client = client;
        }

        public ClientHandler(TcpClient client, string UUID, string Command, string Arguments)
        {
            this.UUID = UUID;
            this.client = client;
            this.Command = Command;
            this.Arguments = Arguments;
        }

        public ClientHandler(string Command, string Arguments)
        {
            this.Command = Command;
            this.Arguments = Arguments;
        }

        public ClientHandler(string Command)
        {
            this.Command = Command;
        }

        public async Task Handle(Usage usage)
        {
            try
            {
                if(usage is Usage.Disposable)
                {
                    while (Server.clientList.ContainsKey(UUID))
                    {
                        string command = null;

                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("\nВведите команду ($download/$upload/$delete) а так же аргументы:");
                        Console.WriteLine("Для выхода из клиента - $exit");
                        Console.ResetColor();

                        do
                        {
                            command = Console.ReadLine();

                        } while (string.IsNullOrEmpty(command) && command.All(c => c == ' '));

                        if (command.Split(' ').Length is 2)
                        {
                            ICommand commandHandler = await GetCommandToClientHandler(client, command.Split(' ')[0], command.Split(' ')[1]);

                            if (commandHandler != null)
                            {
                                await commandHandler.ExecuteAsync(client);
                            }
                        }
                        else if (command is "$exit")
                        {
                            return;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Команда введена не верно");
                            Console.ResetColor();
                            continue;
                        }
                    }
                }
                else if(usage is Usage.Reusable)
                {

                    if(Server.clientList.ContainsKey(UUID))
                    {
                        if (!string.IsNullOrEmpty(Command) && !string.IsNullOrEmpty(Arguments))
                        {
                            ICommand commandHandler = await GetCommandToClientHandler(client, Command, Arguments);

                            if (commandHandler != null)
                            {
                                await commandHandler.ExecuteAsync(client);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Получены некорректный аргументы либо команда");
                            Console.ResetColor();
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Command))
                    {
                        if (!string.IsNullOrEmpty(Arguments))
                        {
                            foreach (var client in Server.clientList)
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine($"\nОтправка команды клиену {client.Key}");
                                Console.ResetColor();
                                ICommand commandHandler = await GetCommandToClientHandler(client.Value, Command, Arguments);

                                if (commandHandler != null)
                                {
                                    await commandHandler.ExecuteAsync(client.Value);
                                }

                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine($"Клиент {client.Key} получил команду");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            if(Command is "$stop")
                            {
                                foreach (var client in Server.clientList)
                                {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine($"\nОтправка команды клиену {client.Key}");
                                    Console.ResetColor();

                                    await MessageServer.SendMessageToClient(client.Value, Command);

                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine($"Клиент {client.Key} получил команду");
                                    Console.ResetColor();
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Получены неверные аргументы");
                                Console.ResetColor();
                            }
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Получена неверная команда");
                        Console.ResetColor();
                        return;
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

        private async Task<ICommand> GetCommandToClientHandler(TcpClient client, string command, string arguments = null)
        {
            switch (command)
            {
                case "$download":

                    string[] PathsDownload = arguments.Split('|');

                    if (File.Exists(PathsDownload[0]) || Directory.Exists(PathsDownload[0]))
                    {
                        await MessageServer.SendMessageToClient(client, $"{command} {PathsDownload[1]}");

                        if(await MessageServer.ReadClientMessage(client) is "$success")
                        {
                            return new ReciveCommand(PathsDownload[0]);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Клиент сообщил о некорректности пути отправки -  {PathsDownload[1]}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Путь для сохранения некорректен - {PathsDownload[0]}");
                    }

                    break;

                case "$upload":

                    string[] PathsUpload = arguments.Split('|');

                    if (File.Exists(PathsUpload[0]) || Directory.Exists(PathsUpload[0]))
                    {
                        await MessageServer.SendMessageToClient(client, $"{command} {PathsUpload[1]}");

                        if(await MessageServer.ReadClientMessage(client) is "$success")
                        {
                            return new SendCommand(PathsUpload[0]);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Клиент сообщил о некорректности пути сохранения - {PathsUpload[1]}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Путь для отправки некорректен - {PathsUpload[0]}");
                    }

                    break;
                case "$delete":

                    await MessageServer.SendMessageToClient(client, $"{command} {arguments}");

                    if (await MessageServer.ReadClientMessage(client) is "$error")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Клиент сообщил о некорректности пути удаления - {arguments}");
                        Console.ResetColor();
                    }

                        break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Неизвестная команда: {command}");
                    Console.ResetColor();
                    return null;
            }

            return null;
        }
    }
}
