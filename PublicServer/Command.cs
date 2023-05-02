using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PublicClient
{

    public interface ICommand
    {
        Task Execute(NetworkStream clientStream);
    }
    public class DownloadCommand : ICommand
    {
        public readonly string SavePath;

        public DownloadCommand(string SavePath)
        {
            this.SavePath = SavePath;
        }

        public async Task Execute(NetworkStream clientStream)
        {
            if (Directory.Exists(SavePath))
            {
                string FileName;
                long fileSize;

                BinaryReader reader = new BinaryReader(clientStream);

                FileName = reader.ReadString();
                fileSize = reader.ReadInt64();

                Console.WriteLine("Получен файл: {0}, размер: {1} байт(а).", FileName, fileSize);

                using (FileStream fileStream = new FileStream(Path.Combine(SavePath, FileName), FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[fileSize + 4096];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while (totalBytesRead < fileSize)
                    {
                        bytesRead = clientStream.Read(buffer, 0, buffer.Length);
                        fileStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                    }
                }

                Console.WriteLine($"Файл {FileName} успешно сохранен на диск.\n");
            }
            else
            {
                await Console.Out.WriteLineAsync("Путь для сохранения некорректен");
            }
        }
    }

    public class UploadCommand : ICommand
    {
        public readonly string SendPath;

        public UploadCommand(string SendPath)
        {
            this.SendPath = SendPath;
        }

        public async Task Execute(NetworkStream clientStream)
        {
            if (File.Exists(SendPath))
            {
                long fileSize;

                BinaryWriter writer = new BinaryWriter(clientStream);

                FileInfo fileInfo = new FileInfo(SendPath);
                fileSize = fileInfo.Length;
                writer.Write(fileInfo.Name);
                writer.Write(fileInfo.Length);

                Console.WriteLine($"Отправлен {fileInfo.Name}, длина - {fileInfo.Length} байт(а)");


                using (FileStream fileStream = new FileStream(SendPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fileSize + 4096];
                    int bytesRead;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await clientStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                Console.WriteLine($"Файл отправлен клиенту\n");
            }
            else
            {
                await Console.Out.WriteLineAsync("Путь для отправки файла некорректен");
            }
        }
    }
}
