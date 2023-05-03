

using PublicClient;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PublicServer
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

                while (true)
                {
                    ICommand commandHandler = (ICommand)await GetCommandHandler(client);
                    commandHandler?.Execute(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
            }
        }

        private async Task<ICommand> GetCommandHandler(TcpClient client)
        {
            if(client.Connected)
            {
                string command = await Server.ReadClientMessage(client);

                switch (command)
                {
                    case "$download":

                        string SendPath = await Server.ReadClientMessage(client);
                        Console.WriteLine($"Сервер получил путь для отправки файла - {SendPath}");

                        if (File.Exists(SendPath) || Directory.Exists(SendPath))
                        {
                            return new SendCommand(SendPath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для отправки клиенту некорректен - {SendPath}");
                        }

                        break;

                    case "$upload":

                        string SavePath = await Server.ReadClientMessage(client);

                        Console.WriteLine($"Сервер получил путь для сохранения файла - {SavePath}");

                        if (Directory.Exists(SavePath))
                        {
                            return new ReciveCommand(SavePath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для получения файла от клиента некорректен - {SavePath}");
                        }

                        break;
                    default:
                        Console.WriteLine($"Неизвестная команда: {command}");
                        return null;
                }
            }

            return null;
        }

    }

}
