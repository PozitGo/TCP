using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static PublicServer.Commans.SendCommand;

namespace PublicServer.Commans
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

                while (client.Connected)
                {
                    ICommand commandHandler = await GetCommandHandler(client);
                    commandHandler?.Execute(stream);
                }

                Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
            }
        }

        private async Task<ICommand> GetCommandHandler(TcpClient client)
        {
            if (client.Connected)
            {
                string command = await Server.ReadClientMessage(client);

                switch (command)
                {
                    case "$download":

                        string SendPath = await Server.ReadClientMessage(client);

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

                        if (Directory.Exists(SavePath))
                        {
                            return new ReciveCommand(SavePath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для получения файла от клиента некорректен - {SavePath}");
                        }

                        break;

                    case "$delete":

                        string ReciveDeletePath = await Server.ReadClientMessage(client);

                        if (Directory.Exists(ReciveDeletePath) || File.Exists(ReciveDeletePath))
                        {
                            Console.WriteLine("Полученный путь норм" + ReciveDeletePath);
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
            }

            return null;
        }

    }

}
