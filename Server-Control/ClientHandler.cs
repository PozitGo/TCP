using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server_Control
{
    public class ClientHandler
    {
        private readonly TcpClient client;

        public ClientHandler(TcpClient client)
        {
            this.client = client;
        }

        public async Task Handle()
        {
            try
            {

                NetworkStream stream = client.GetStream();
                string command = default;


                while (command != "$exit client" && Server.clientList.ContainsKey(((IPEndPoint)client.Client.RemoteEndPoint).Address))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\nВведите команду ($download/$upload/$delete):");
                    Console.WriteLine("Для выхода из клиента - $exit client");
                    Console.ResetColor();

                    do
                    {
                        command = Console.ReadLine();

                    } while (string.IsNullOrEmpty(command) && command.All(c => c == ' '));

                    if (command != "$exit client")
                    {
                        ICommand commandHandler = default;
                        Task.Factory.StartNew(async () => commandHandler = await GetCommandToClientHandler(command, client)).Wait();

                        if (commandHandler != null)
                        {
                            await commandHandler.ExecuteAsync(client);
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

        private async Task<ICommand> GetCommandToClientHandler(string command, TcpClient client)
        {
            switch (command)
            {
                case "$download":

                    await MessageServer.SendMessageToClient(client, command);
                    bool successDownload = false;

                    while (!successDownload)
                    {
                        Console.WriteLine("Использование: <Путь для сохранения на локальном пк> | <Путь для извлечения на удалённом пк>");
                        Console.WriteLine("Для выхода из ввода - $exit");

                        string tempDownload = Console.ReadLine();
                        if (tempDownload != "$exit")
                        {
                            string[] PathsDownload = tempDownload.Split('|');


                            if (Directory.Exists(PathsDownload[0]))
                            {
                                await MessageServer.SendMessageToClient(client, PathsDownload[1]);
                                successDownload = true;
                                return new ReciveCommand(PathsDownload[0]);
                            }
                            else
                            {
                                Console.WriteLine($"Путь для сохранения некорректен - {PathsDownload[0]}");
                                successDownload = false;
                            }
                        }
                        else
                        {

                            await MessageServer.SendMessageToClient(client, tempDownload);
                            successDownload = true;

                            return null;
                        }
                    }

                    break;

                case "$upload":

                    await MessageServer.SendMessageToClient(client, command);
                    bool successUpload = false;

                    while (!successUpload)
                    {
                        Console.WriteLine("Использование: <Путь для отправки на локальном пк> | <Путь для сохранения на удалённом пк>");
                        Console.WriteLine("Для выхода из ввода - $exit");

                        string tempUpload = Console.ReadLine();

                        if (tempUpload != "$exit")
                        {
                            string[] PathsUpload = tempUpload.Split('|');

                            if (File.Exists(PathsUpload[0]) || Directory.Exists(PathsUpload[0]))
                            {
                                await MessageServer.SendMessageToClient(client, PathsUpload[1]);
                                successUpload = true;
                                return new SendCommand(PathsUpload[0]);
                            }
                            else
                            {
                                Console.WriteLine($"Путь для отправки некорректен - {PathsUpload[0]}");
                                successUpload = false;
                            }

                        }
                        else
                        {

                            await MessageServer.SendMessageToClient(client, tempUpload);
                            successDownload = true;
                            return null;
                        }
                    }

                    break;
                case "$delete":

                    await MessageServer.SendMessageToClient(client, command);

                    Console.WriteLine("Использование: <Путь для запроса на удаление на удалённый пк>");
                    Console.WriteLine("Для выхода из ввода - $exit");

                    string tempDelete = Console.ReadLine();

                    await MessageServer.SendMessageToClient(client, tempDelete);

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
