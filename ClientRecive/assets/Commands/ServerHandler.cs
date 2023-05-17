using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClientRecive.assets.Commands
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
                string[] Data = default;
                string Value = await Client.ReceiveMessageFromServer(client);

                if (Value != "$stop")
                {
                    Data = Value.Split(' ');
                }
                else
                {
                    client.Close();
                    await Console.Out.WriteLineAsync("\nСервер сообщил о прекращении работы, начало переподключения...");
                    Thread.Sleep(5000);
                    await Start.client.Connect();
                }

                switch (Data[0])
                {
                    case "$download":

                        string SendPath = Data[1];

                        if (File.Exists(SendPath) || Directory.Exists(SendPath))
                        {
                            await Client.SendClientMessage(client, "$success");
                            return new SendCommand(SendPath);
                        }
                        else
                        {
                            await Client.SendClientMessage(client, "$error");
                            Console.WriteLine($"Путь для отправки клиенту некорректен - {SendPath}");
                        }

                        break;

                    case "$upload":

                        string SavePath = Data[1];

                        if (Directory.Exists(SavePath))
                        {
                            await Client.SendClientMessage(client, "$success");
                            return new ReciveCommand(SavePath);
                        }
                        else
                        {
                            await Client.SendClientMessage(client, "$error");
                            Console.WriteLine($"Путь для получения файла от клиента некорректен - {SavePath}");
                        }

                        break;

                    case "$delete":

                        string ReciveDeletePath = Data[1];

                        if (Directory.Exists(ReciveDeletePath) || File.Exists(ReciveDeletePath))
                        {
                            await Client.SendClientMessage(client, "$success");
                            return new DeleteReciveCommand(ReciveDeletePath);
                        }
                        else
                        {
                            await Client.SendClientMessage(client, "$error");
                            Console.WriteLine($"Путь для удаления файла от клиента некорректен - {ReciveDeletePath}");
                        }

                        break;
                    default:
                        Console.WriteLine($"Неизвестная команда: {Data[0]}");
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
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
                Console.ResetColor();
            }

            return null;
        }

    }
}
