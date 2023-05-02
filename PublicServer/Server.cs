using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PublicServer
{
    public class Server
    {
        private TcpListener server;
        private readonly IPAddress ServerIP;
        private readonly int ServerPort;

        public Server(string ServerIP, int ServerPort)
        {
            this.ServerIP = IPAddress.Parse(ServerIP);
            this.ServerPort = ServerPort;
        }

        public void Start()
        {
            server = new TcpListener(ServerIP, ServerPort);

            server.Start();
            Console.WriteLine("Сервер запущен...\nОжидает подключения клиента.");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключился.");
                Task.Factory.StartNew(() => HandleConnection(client));
            }
        }

        private async void HandleConnection(TcpClient client)
        {
            try
            {
                ClientHandler clientHandler = new ClientHandler(client);
                await clientHandler.Handle();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
            }
        }

        public static async Task<string> ReadClientMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }

        }
    }
}
