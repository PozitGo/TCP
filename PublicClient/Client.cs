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
        private bool isAuthorize;

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
                while (!isAuthorize)
                {
                    RsaEncryptor encryptor = new RsaEncryptor(RSAKeySerializer.DeserializeFromString(await ReceiveMessageFromServer(_client)));
                    await SendClientMessage(_client, encryptor.Encrypt(GetPasswordFromConsole()));

                    switch (await ReceiveMessageFromServer(_client))
                    {
                        case "$success":
                            isAuthorize = true;
                            Console.WriteLine("\nАвторизация успешна.");
                            break;
                        case "$error":
                            isAuthorize = false;
                            Console.WriteLine("\nНеверный пароль");
                            break;
                    }
                }

                string command;
                Console.WriteLine("Введите команду ($download/$upload/$delete):");

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

        public async Task<string> ReceiveMessageFromServer(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[70000];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); ;
                    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }
        }


        public static async Task SendClientMessage(TcpClient client, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static string GetPasswordFromConsole()
        {
            Console.Write("Введите пароль: ");
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);


            return password;
        }

        private async Task<ICommand> GetCommandHandler(string command, TcpClient client)
        {
            switch (command)
            {
                case "$download":

                    await SendClientMessage(client, command);
                    Console.WriteLine("Использование: <Путь для сохранения> | <Путь для извлечения>");
                    Console.WriteLine("Для выхода из ввода - $exit");

                    string tempDownload = Console.ReadLine();

                    if (tempDownload != "$exit")
                    {
                        string[] PathsDownload = tempDownload.Split('|');


                        if (Directory.Exists(PathsDownload[0]))
                        {
                            await SendClientMessage(client, PathsDownload[1]);
                            return new ReciveCommand(PathsDownload[0]);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для сохранения некорректен - {PathsDownload[0]}");
                        }
                    }

                    break;

                case "$upload":

                    await SendClientMessage(client, command);
                    Console.WriteLine("Использование: <Путь для отправки> | <Путь для сохранения>");
                    Console.WriteLine("Для выхода из ввода - $exit");

                    string tempUpload = Console.ReadLine();

                    if (tempUpload != "$exit")
                    {
                        string[] PathsUpload = tempUpload.Split('|');

                        if (File.Exists(PathsUpload[0]) || Directory.Exists(PathsUpload[0]))
                        {
                            await SendClientMessage(client, PathsUpload[1]);
                            return new SendCommand(PathsUpload[0]);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для отправки некорректен - {PathsUpload[0]}");
                        }

                    }

                    break;
                case "$delete":

                    await SendClientMessage(client, command);
                    Console.WriteLine("Использование: <Путь для запроса на удаление>");
                    Console.WriteLine("Для выхода из ввода - $exit");

                    string tempDelete = Console.ReadLine();

                    if (tempDelete != "$exit")
                    {
                        await SendClientMessage(client, tempDelete);
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
