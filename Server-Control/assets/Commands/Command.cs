using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server_Control.assets.Commands
{
    public interface ICommand
    {
        Task ExecuteAsync(TcpClient client);
    }
    public class ReciveCommand : ICommand
    {
        public string SavePath;
        private string CurrentDirectory;
        private ProgressTracker progressTracker;
        private bool IsFirstIteration;
        public ReciveCommand(string SavePath)
        {
            this.SavePath = SavePath;
            CurrentDirectory = SavePath;
            progressTracker = new ProgressTracker();
            IsFirstIteration = true;
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());

                string Name = reader.ReadString();

                if (Name.Contains("."))
                {
                    if (IsFirstIteration) 
                    {
                        IsFirstIteration = false;
                        await ReciveFileAsync(client, Path.Combine(CurrentDirectory, Name), progressTracker, true);
                    }
                    else
                    {
                        IsFirstIteration = false;
                        await ReciveFileAsync(client, Path.Combine(CurrentDirectory, Name), progressTracker);
                    }
                }
                else
                {
                    IsFirstIteration = false;

                    progressTracker.TotalBytes = reader.ReadInt64();

                    Console.Write($"Размер принимаемой единицы - ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{Math.Round(progressTracker.TotalBytes / (double)(1024 * 1024), 2)} Мб\n");
                    Console.ResetColor();

                    await ReciveDirectoryAsync(client, Name);
                }
            }
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 1 " + ex.Message);
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
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 2 " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveFileAsync(TcpClient client, string file, ProgressTracker progressTracker, bool IsFullFile = false)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());
                long fileSize = reader.ReadInt64();

                if(IsFullFile)
                {
                    progressTracker.TotalBytes = fileSize;

                    Console.Write($"Размер принимаемой единицы - ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{Math.Round(progressTracker.TotalBytes / (double)(1024 * 1024), 2)} Мб\n");
                    Console.ResetColor();
                }

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

                        progressTracker.GetProgress(bytesRead, ProgressPerforms.Recive);
                    }
                }
            }
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 3 " + ex.Message);
                Console.ResetColor();
            }
        }
    }

    public class SendCommand : ICommand
    {
        public readonly string SendPath;
        private ProgressTracker progressTracker;

        public SendCommand(string SendPath)
        {
            this.SendPath = SendPath;
            progressTracker = new ProgressTracker(SendPath);
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            try
            {
                if (File.Exists(SendPath))
                {
                    await SendFileAsync(client, SendPath, Path.GetFileName(SendPath), true);
                }
                else
                {
                    await SendDirectoryAsync(client, SendPath, Path.GetFileName(SendPath));
                }
            }
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 4 " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task SendDirectoryAsync(TcpClient client, string directory, string directoryName)
        {
            try
            {
                BinaryWriter writer = new BinaryWriter(client.GetStream());

                long SizeDirectory = GetDirectorySize(directory);
                writer.Write(directoryName);
                writer.Write(SizeDirectory);
                writer.Write(Directory.GetDirectories(directory).Length);
                writer.Write(Directory.GetFiles(directory).Length);

                Console.Write($"Размер отправляемой еденицы - ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{Math.Round(SizeDirectory / (double)(1024 * 1024), 2)} Мб\n");
                Console.ResetColor();

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
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 5 " + ex.Message);
                Console.ResetColor();
            }
        }
        public async Task SendFileAsync(TcpClient client, string file, string fileName, bool IsFullFile = false)
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

                    if(IsFullFile) 
                    {
                        Console.Write($"Размер отправляемой еденицы - ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{Math.Round(progressTracker.TotalBytes / (double)(1024 * 1024), 2)} Мб\n");
                        Console.ResetColor();
                    }

                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        while (totalBytesSent < fileSize)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                            totalBytesSent += bytesRead;

                            await client.GetStream().WriteAsync(buffer, 0, bytesRead);
                            bytesSent = bytesRead;

                            progressTracker.GetProgress(bytesRead, ProgressPerforms.Send);
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
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 6 " + ex.Message);
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
}