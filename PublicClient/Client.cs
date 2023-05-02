using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PublicClient
{
    public class Client
    {
        private TcpClient _client;
        private readonly IPAddress ipClient;
        private readonly int portClient;

        public Client(IPAddress iPAddress, int portClient)
        {
            this.ipClient = iPAddress;
            this.portClient = portClient;
        }

        public async Task Connect()
        {
            Console.Out.WriteLine("Клиент запускается...");

            _client = new TcpClient(ipClient.ToString(), portClient);
            Console.WriteLine($"Подключен к серверу");

            while (true)
            {
                string command;
                Console.WriteLine("Введите команду ($download/$upload):");

                do
                {
                    command = Console.ReadLine();

                } while (string.IsNullOrEmpty(command) && command.All(c => c == ' '));


                if (_client.Connected)
                {
                    ICommand commandHandler = await GetCommandHandler(command, _client);
                    commandHandler?.Execute(_client.GetStream());
                }
            }
        }


        public static async Task SendClientMessage(TcpClient client, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task<ICommand> GetCommandHandler(string command, TcpClient client)
        {
            await SendClientMessage(client, command);

            switch (command)
            {
                case "$download":
                    Console.WriteLine("Использование: <Путь для сохранения> | <Путь для извлечения>");

                    string tempDownload = Console.ReadLine();
                    string[] PathsDownload = tempDownload.Split('|');

                    Console.WriteLine($"Путь сохранения - {PathsDownload[0]}, путь отправки на сервер - {PathsDownload[1]}");

                    if (Directory.Exists(PathsDownload[0]))
                    {
                        await SendClientMessage(client, PathsDownload[1]);
                        return new DownloadCommand(PathsDownload[0]);
                    }
                    else
                    {
                        Console.WriteLine($"Путь для сохранения некорректен - {PathsDownload[0]}");
                    }
                    break;

                case "$upload":

                    Console.WriteLine("Использование: <Путь для отправки> | <Путь для сохранения>");

                    string tempUpload = Console.ReadLine();
                    string[] PathsUpload = tempUpload.Split('|');

                    Console.WriteLine($"Путь отправки на сервер - {PathsUpload[0]}, путь для сохранения - {PathsUpload[1]}");

                    if (File.Exists(PathsUpload[0]))
                    {
                        await SendClientMessage(client, PathsUpload[1]);
                        return new UploadCommand(PathsUpload[0]);
                    }
                    else
                    {
                        Console.WriteLine($"Путь для отправки некорректен - {PathsUpload[0]}");
                    }

                    break;

                default:
                    Console.WriteLine($"Неизвестная команда: {command}");
                    return null;
            }

            return null;
        }
    }
}
