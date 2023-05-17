using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientRecive.assets.Commands;

namespace ClientRecive.assets
{
    public class Client
    {
        private TcpClient _client;
        private readonly IPAddress ipClient;
        private readonly int portClient;
        private bool _isFirstConnect = true;
        public Client(IPAddress iPAddress, int portClient)
        {
            ipClient = iPAddress;
            this.portClient = portClient;
        }

        public async Task Connect()
        {
            if (_isFirstConnect)
            {
                await Console.Out.WriteLineAsync("Клиент запускается...");
                _isFirstConnect = false;
            }

            while (true)
            {
                try
                {
                    _client = new TcpClient(ipClient.ToString(), portClient);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Подключен к серверу");
                    Console.ResetColor();

                    await HandleConnection(_client);
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
        }

        private async Task HandleConnection(TcpClient client)
        {
            try
            {
                while (true)
                {
                    ServerHandler clientHandler = new ServerHandler(client);
                    await clientHandler.Handle();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
                await Console.Out.WriteLineAsync("Соединение с сервером потеряно, попытка переподключиться...");
                Console.ResetColor();
                await Connect();
            }
        }

        public static async Task<string> ReceiveMessageFromServer(TcpClient client)
        {
            try
            {

                NetworkStream stream = client.GetStream();

                while (client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        string base64String = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        byte[] bytes = Convert.FromBase64String(base64String);

                        return Encoding.UTF8.GetString(bytes);
                    }
                }

                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();

            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();
            }

            return null;
        }

        public static async Task SendClientMessage(TcpClient client, string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться...");
                await Start.client.Connect();
            }
        }
    }
}
