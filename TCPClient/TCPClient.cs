using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    class TcpFileTransferClient
    {
        private readonly string serverAddress;
        private readonly int serverPort;
        public TcpFileTransferClient(string serverAddress, int serverPort)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
        }

        public async Task SendFile(string filePath)
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(serverAddress, serverPort);
                Console.WriteLine($"Подключено к серверу для отправки файлов: {serverAddress}:{serverPort}");

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] commandBuffer = Encoding.ASCII.GetBytes("p");
                    await stream.WriteAsync(commandBuffer, 0, commandBuffer.Length);

                    byte[] fileNameBuffer = Encoding.ASCII.GetBytes(Path.GetFileName(filePath));
                    await stream.WriteAsync(fileNameBuffer, 0, fileNameBuffer.Length);

                    // Читаем ответ сервера
                    byte[] responseBuffer = new byte[1];
                    while (await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length) > 0)
                    {
                        if (responseBuffer[0] == 1) // Ожидаемый ответ сервера
                        {
                            await SendFileData(stream, filePath);
                            break;
                        }
                    }
                }
            }
        }

        public async Task ReceiveFile(string fileName, string savePath)
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(serverAddress, serverPort);
                Console.WriteLine($"Подключено к серверу для получения файлов: {serverAddress}:{serverPort}");

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] commandBuffer = Encoding.ASCII.GetBytes("g");
                    await stream.WriteAsync(commandBuffer, 0, commandBuffer.Length);

                    byte[] fileNameBuffer = Encoding.ASCII.GetBytes(fileName);
                    await stream.WriteAsync(fileNameBuffer, 0, fileNameBuffer.Length);

                    // Читаем ответ сервера
                    byte[] responseBuffer = new byte[1];
                    while (await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length) > 0)
                    {
                        if (responseBuffer[0] == 1) // Ожидаемый ответ сервера
                        {
                            await ReceiveFileData(stream, savePath);
                            break;
                        }
                    }
                }
            }

        }

        private async Task SendFileData(NetworkStream stream, string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }

        private async Task ReceiveFileData(NetworkStream stream, string savePath)
        {
            using (FileStream fileStream = File.Create(savePath))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
    }
}