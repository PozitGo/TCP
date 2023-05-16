using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientRecive
{
    public class ServerHandler
    {
        private readonly TcpClient client;

        public ServerHandler(TcpClient client)
        {
            this.client = client;
        }

        public async Task Handle()
        {
            try
            {
                while (true)
                {
                    ICommand commandHandler = await GetCommandHandler(client);

                    if (commandHandler != null)
                    {
                        await commandHandler.ExecuteAsync(client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
            }
        }

        private async Task<ICommand> GetCommandHandler(TcpClient client)
        {
            try
            {
                string command = await Client.ReceiveMessageFromServer(client);

                switch (command)
                {
                    case "$download":

                        string SendPath = await Client.ReceiveMessageFromServer(client);

                        if (SendPath is "$exit")
                        {
                            return null;
                        }
                        else if (File.Exists(SendPath) || Directory.Exists(SendPath))
                        {
                            return new SendCommand(SendPath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для отправки клиенту некорректен - {SendPath}");
                        }

                        break;

                    case "$upload":

                        string SavePath = await Client.ReceiveMessageFromServer(client);

                        if (SavePath is "$exit")
                        {
                            return null;
                        }
                        else if (Directory.Exists(SavePath))
                        {
                            return new ReciveCommand(SavePath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для получения файла от клиента некорректен - {SavePath}");
                        }

                        break;

                    case "$delete":

                        string ReciveDeletePath = await Client.ReceiveMessageFromServer(client);

                        if (ReciveDeletePath is "$exit")
                        {
                            return null;
                        }
                        else if (Directory.Exists(ReciveDeletePath) || File.Exists(ReciveDeletePath))
                        {
                            return new DeleteReciveCommand(ReciveDeletePath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для удаления файла от клиента некорректен - {ReciveDeletePath}");
                        }

                        break;
                    default:
                        Console.WriteLine($"Неизвестная команда: {command}");
                        return null;
                }


                return null;
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка " + ex.Message);
                Console.ResetColor();
            }

            return null;
        }

    }
}
