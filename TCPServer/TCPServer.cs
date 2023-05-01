using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    class TcpFileTransferServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;

        public TcpFileTransferServer(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public async Task Start()
        {
            TcpListener listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine($"Сервер запущен. Ожидание подключений на {ipAddress}:{port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"Новое подключение: {client.Client.RemoteEndPoint}");

                await Task.Run(async() => await HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] commandBuffer = new byte[1];
                await stream.ReadAsync(commandBuffer, 0, commandBuffer.Length);
                string command = Encoding.ASCII.GetString(commandBuffer);

                byte[] fileNameBuffer = new byte[256];
                await stream.ReadAsync(fileNameBuffer, 0, fileNameBuffer.Length);
                string fileName = Encoding.ASCII.GetString(fileNameBuffer).TrimEnd('\0');

                Console.WriteLine($"Команда: {command}, файл: {fileName}");

                if (command == "p")
                {
                    await ReceiveFileData(stream, fileName);
                }
                else if (command == "g")
                {
                    await SendFileData(stream, fileName);
                }
            }

            client.Close();
        }

        private async Task ReceiveFileData(NetworkStream stream, string fileName)
        {
            using (FileStream fileStream = File.Create(fileName))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }

        private async Task SendFileData(NetworkStream stream, string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Файл {fileName} не найден");
                return;
            }

            byte[] buffer = new byte[8192];
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
    }
}
