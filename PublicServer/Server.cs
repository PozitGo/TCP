using PublicServer.Commans;
using PublicServer.JSON;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PublicServer
{
    public class Server
    {
        private readonly IPAddress ServerIP;
        private readonly int ServerPort;
        private readonly JsonEncryptionService encryptionService;
        private TcpListener server;

        public Server(string ServerIP, int ServerPort, JsonEncryptionService encryptionService)
        {
            this.ServerIP = IPAddress.Parse(ServerIP);
            this.encryptionService = encryptionService;
            this.ServerPort = ServerPort;
        }

        public Task Start()
        {
            server = new TcpListener(ServerIP, ServerPort);
            server.Start();

            Console.WriteLine("Сервер запущен...\nОжидает подключения клиента.");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключился в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();

                _ = Task.Factory.StartNew(() => Server.CheckConnection(client));
                _ = Task.Factory.StartNew(() => HandleConnection(client));
            }
        }

        public static void CheckConnection(TcpClient client)
        {
            while (true)
            {
                Thread.Sleep(10000);

                try
                {
                    NetworkStream stream = client.GetStream();

                    Socket socket = client.Client;
                    bool isConnected = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

                    if (!isConnected)
                    {
                        Console.ForegroundColor= ConsoleColor.DarkCyan;
                        Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                        Console.ResetColor();
                        break;
                    }
                }
                catch (SocketException)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} отключился в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                    Console.ResetColor();
                    break;
                }
            }
        }

        private async void HandleConnection(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    ClientHandler clientHandler = new ClientHandler(client, encryptionService);
                    await clientHandler.Handle();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static async Task SendMessageToClient(TcpClient client, string message)
        {
            if (client.Connected)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
            }
        }

        public static async Task<string> ReadClientMessage(TcpClient client)
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
    }
}
