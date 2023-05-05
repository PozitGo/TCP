using PublicServer.Commans;
using PublicServer.Decryptor;
using PublicServer.JSON;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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

        public async Task Start()
        {
            server = new TcpListener(ServerIP, ServerPort);

            server.Start();
            Console.WriteLine("Сервер запущен...\nОжидает подключения клиента.");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключился.");

                _ = Task.Factory.StartNew(() => HandleConnection(client));
            }
        }

        private void CheckConnection(TcpClient client)
        {
            while (true)
            {
                Thread.Sleep(1000);

                try
                {
                    Socket socket = client.Client;
                    bool isConnected = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

                    if (!isConnected)
                    {
                        Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                        client.Close();
                        break;
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                    client.Close();
                    break;
                }
            }
        }



        private async void HandleConnection(TcpClient client)
        {
            try
            {
                _ = Task.Factory.StartNew(() => CheckConnection(client));

                while (client.Connected)
                {
                    (RSAParameters publicKey, RSAParameters privateKey) = RsaKeyGenerator.GenerateKeyPair();
                    RsaDecryptor decryptor = new RsaDecryptor(privateKey);

                    await SendMessageToClient(client, RSAKeySerializer.SerializeToString(publicKey));

                    while (decryptor.Decrypt(await ReadClientMessage(client)) != encryptionService.DecryptJsonFromFile())
                    {
                        if (client.Connected)
                        {
                            await SendMessageToClient(client, "$error");
                            (publicKey, privateKey) = RsaKeyGenerator.GenerateKeyPair();
                            decryptor = new RsaDecryptor(privateKey);
                            await SendMessageToClient(client, RSAKeySerializer.SerializeToString(publicKey));
                        }
                        else
                        {
                            Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                            client.Close();
                        }

                    }

                    await SendMessageToClient(client, "$success");

                    ClientHandler clientHandler = new ClientHandler(client);
                    await clientHandler.Handle();
                }

                Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                client.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
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
                client.Close();
            }
        }

        public static async Task<string> ReadClientMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (client.Connected)
            {
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[70000];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }

            Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
            client.Close();
            return null;
        }
    }
}
