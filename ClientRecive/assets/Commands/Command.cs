using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientRecive.assets.Commands
{
    public interface ICommand
    {
        Task ExecuteAsync(TcpClient client);
    }
    public class ReciveCommand : ICommand
    {
        public string SavePath;
        private string CurrentDirectory;

        public ReciveCommand(string SavePath)
        {
            this.SavePath = SavePath;
            CurrentDirectory = SavePath;
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());

                string Name = reader.ReadString();

                if (Name.Contains("."))
                {
                    await ReciveFileAsync(client, Path.Combine(CurrentDirectory, Name));
                }
                else
                {
                    long TotalBytes = reader.ReadInt64();
                    await ReciveDirectoryAsync(client, Name);
                }
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка 1" + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveDirectoryAsync(TcpClient client, string directory)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());

                int CountDirectories = reader.ReadInt32();
                int CountFiles = reader.ReadInt32();

                CurrentDirectory = Path.Combine(CurrentDirectory, directory);
                Directory.CreateDirectory(CurrentDirectory);

                for (int i = 0; i < CountDirectories; i++)
                {
                    await ExecuteAsync(client);
                }

                for (int i = 0; i < CountFiles; i++)
                {
                    await ExecuteAsync(client);
                }

                CurrentDirectory = Directory.GetParent(CurrentDirectory).FullName;
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка 2" + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveFileAsync(TcpClient client, string file)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());
                long fileSize = reader.ReadInt64();

                using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[65536];
                    long totalBytesReceived = 0;
                    int bytesRead;

                    while (totalBytesReceived < fileSize)
                    {
                        int bytesToRead = (int)Math.Min(fileSize - totalBytesReceived, buffer.Length);
                        bytesRead = await client.GetStream().ReadAsync(buffer, 0, bytesToRead);
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesReceived += bytesRead;
                    }
                }
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка 3" + ex.Message);
                Console.ResetColor();
            }
        }
    }

    public class SendCommand : ICommand
    {
        public readonly string SendPath;

        public SendCommand(string SendPath)
        {
            this.SendPath = SendPath;
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            if (File.Exists(SendPath))
            {
                await SendFileAsync(client, SendPath, Path.GetFileName(SendPath));
            }
            else
            {
                await SendDirectoryAsync(client, SendPath, Path.GetFileName(SendPath));
            }
        }

        public async Task SendDirectoryAsync(TcpClient client, string directory, string directoryName)
        {
            try
            {
                BinaryWriter writer = new BinaryWriter(client.GetStream());

                writer.Write(directoryName);
                writer.Write(GetDirectorySize(directory));
                writer.Write(Directory.GetDirectories(directory).Length);
                writer.Write(Directory.GetFiles(directory).Length);

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    string subDirectoryName = Path.GetFileName(subDirectory);
                    await SendDirectoryAsync(client, subDirectory, subDirectoryName);
                }

                foreach (string fileName in Directory.GetFiles(directory))
                {
                    await SendFileAsync(client, fileName, Path.GetFileName(fileName));
                }
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка 4" + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task SendFileAsync(TcpClient client, string file, string fileName)
        {
            try
            {
                if (File.Exists(file))
                {
                    BinaryWriter writer = new BinaryWriter(client.GetStream());

                    FileInfo fileInfo = new FileInfo(file);
                    long fileSize = fileInfo.Length;
                    long totalBytesSent = 0;
                    int bytesSent;

                    writer.Write(fileName);
                    writer.Write(fileSize);

                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        while (totalBytesSent < fileSize)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                            totalBytesSent += bytesRead;

                            await client.GetStream().WriteAsync(buffer, 0, bytesRead);
                            bytesSent = bytesRead;
                        }
                    }
                }
                else
                {
                    await Console.Out.WriteLineAsync("Файл не найден по указанному пути");
                }
            }
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await Start.client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка 5" + ex.Message);
                Console.ResetColor();
            }
        }

        public static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo folder = new DirectoryInfo(folderPath);

            long size = 0;

            foreach (FileInfo file in folder.GetFiles())
            {
                size += file.Length;
            }

            foreach (DirectoryInfo subFolder in folder.GetDirectories())
            {
                size += GetDirectorySize(subFolder.FullName);
            }

            return size;
        }
    }

    public class DeleteReciveCommand : ICommand
    {
        public readonly string RecivePath;

        public DeleteReciveCommand(string RecivePath)
        {
            this.RecivePath = RecivePath;
        }

        public Task ExecuteAsync(TcpClient client)
        {

            if (RecivePath.Contains("."))
            {
                File.Delete(RecivePath);
                Console.WriteLine($"Файл - {Path.GetFileName(RecivePath)} удалён.");
            }
            else
            {
                Console.WriteLine($"Папка - {Path.GetFileName(RecivePath)} удалена.");
                Directory.Delete(RecivePath, true);
            }

            return Task.CompletedTask;
        }
    }

}
