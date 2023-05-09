using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PublicClient.ReciveCommand;

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
            bool isAuthorize = false;
            Console.Out.WriteLine("Клиент запускается...");

            while (true)
            {
                try
                {
                    _client = new TcpClient(ipClient.ToString(), portClient);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Подключен к серверу");
                    Console.ResetColor();
                    break;
                }
                catch (SocketException)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Не удалось подключиться к серверу, попытка через 5 секунд...");
                    Console.ResetColor();
                    Thread.Sleep(5000);
                }
            }

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

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nАвторизация успешна.");
                            Console.ResetColor();

                            break;
                        case "$error":
                            isAuthorize = false;

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nНеверный пароль");
                            Console.ResetColor();

                            int currentLineCursor = Console.CursorTop;
                            Console.SetCursorPosition(0, Console.CursorTop - 2);
                            Console.Write(new string(' ', Console.WindowWidth));
                            Console.SetCursorPosition(0, currentLineCursor - 2);
                            break;
                    }

                }

                string command;

                Console.WriteLine("\nВведите команду ($download/$upload/$delete):");

                do
                {
                    command = Console.ReadLine();

                } while (string.IsNullOrEmpty(command) && command.All(c => c == ' '));

                ICommand commandHandler = await GetCommandHandler(command, _client);

                if (commandHandler != null)
                {
                    await commandHandler.ExecuteAsync(_client.GetStream());
                }

            }
        }

        public async Task<string> ReceiveMessageFromServer(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] buffer = new byte[70000];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    }
                }
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Connect();
            }

            return null;
        }

        public async Task SendClientMessage(TcpClient client, string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Connect();
            }
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
            while (key.Key != ConsoleKey.Enter && !(password.Length > 30));

            return password;
        }

        private async Task<ICommand> GetCommandHandler(string command, TcpClient client)
        {
            switch (command)
            {
                case "$download":

                    await SendClientMessage(client, command);
                    bool successDownload = false;

                    while (!successDownload)
                    {
                        Console.WriteLine("Использование: <Путь для сохранения на локальном пк> | <Путь для извлечения на сервере>");
                        Console.WriteLine("Для выхода из ввода - $exit");

                        string tempDownload = Console.ReadLine();
                        if (tempDownload != "$exit")
                        {
                            string[] PathsDownload = tempDownload.Split('|');


                            if (Directory.Exists(PathsDownload[0]))
                            {
                                await SendClientMessage(client, PathsDownload[1]);
                                successDownload = true;
                                return new ReciveCommand(PathsDownload[0], new Client(ipClient, portClient));
                            }
                            else
                            {
                                Console.WriteLine($"Путь для сохранения некорректен - {PathsDownload[0]}");
                                successDownload = false;
                            }
                        }
                        else
                        {

                            await SendClientMessage(client, tempDownload);
                            successDownload = true;
                            
                            return null;
                        }
                    }

                    break;

                case "$upload":

                    await SendClientMessage(client, command);
                    bool successUpload = false;

                    while (!successUpload)
                    {
                        Console.WriteLine("Использование: <Путь для отправки на локальном пк> | <Путь для сохранения на сервере>");
                        Console.WriteLine("Для выхода из ввода - $exit");

                        string tempUpload = Console.ReadLine();

                        if (tempUpload != "$exit")
                        {
                            string[] PathsUpload = tempUpload.Split('|');

                            if (File.Exists(PathsUpload[0]) || Directory.Exists(PathsUpload[0]))
                            {
                                await SendClientMessage(client, PathsUpload[1]);
                                successUpload = true;
                                return new SendCommand(PathsUpload[0], new Client(ipClient, portClient));
                            }
                            else
                            {
                                Console.WriteLine($"Путь для отправки некорректен - {PathsUpload[0]}");
                                successUpload = false;
                            }

                        }
                        else
                        {

                            await SendClientMessage(client, tempUpload);
                            successDownload = true;
                            return null;
                        }
                    }

                    break;
                case "$delete":

                    await SendClientMessage(client, command);

                    Console.WriteLine("Использование: <Путь для запроса на удаление на сервер>");
                    Console.WriteLine("Для выхода из ввода - $exit");

                    string tempDelete = Console.ReadLine();

                    await SendClientMessage(client, tempDelete);

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
