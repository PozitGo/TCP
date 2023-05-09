using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server_Control
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
                ProgressTracker progressTracker = new ProgressTracker();

                string Name = MessageServer.ReadClientMessage(client).Result;

                if (Name.Contains("."))
                {
                    await ReciveFileAsync(client, Path.Combine(CurrentDirectory, Name), progressTracker);
                }
                else
                {
                    progressTracker.TotalBytes = Convert.ToInt32(MessageServer.ReadClientMessage(client).Result);
                    await ReciveDirectoryAsync(client, Name);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveDirectoryAsync(TcpClient client, string directory)
        {
            try
            {
                int CountFiles = Convert.ToInt16(MessageServer.ReadClientMessage(client).Result);
                int CountDirectories = Convert.ToInt16(MessageServer.ReadClientMessage(client).Result);

                CurrentDirectory = Path.Combine(CurrentDirectory, directory);
                await Console.Out.WriteLineAsync($"Получен каталог {directory}");
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
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveFileAsync(TcpClient client, string file, ProgressTracker progressTracker)
        {
            try
            {
                long fileSize = Convert.ToInt16(MessageServer.ReadClientMessage(client).Result);

                if (fileSize > 250 * 1024 * 1024)
                {
                    using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[150 * 1024 * 1024];
                        long totalBytesReceived = 0;
                        int bytesRead;

                        while (totalBytesReceived < fileSize)
                        {
                            int bytesToRead = (int)Math.Min(fileSize - totalBytesReceived, buffer.Length);
                            bytesRead = await client.GetStream().ReadAsync(buffer, 0, bytesToRead);
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesReceived += bytesRead;

                            await Task.Factory.StartNew(() => progressTracker.ProgressRecive(totalBytesReceived));
                        }
                    }
                }
                else
                {
                    using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[fileSize];
                        int bytesRead;
                        long totalBytesRead = 0;

                        while (totalBytesRead < fileSize)
                        {
                            bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;


                            await Task.Factory.StartNew(() => progressTracker.ProgressRecive(totalBytesRead));
                        }
                    }
                }

                Console.WriteLine($"Файл {file} успешно сохранен на диск.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
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
            try
            {
                if (File.Exists(SendPath))
                {
                    ProgressTracker progressTracker = new ProgressTracker(SendPath);

                    await SendFileAsync(client, SendPath, Path.GetFileName(SendPath), progressTracker);
                }
                else
                {
                    await SendDirectoryAsync(client, SendPath, Path.GetFileName(SendPath));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка" + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task SendDirectoryAsync(TcpClient client, string directory, string directoryName)
        {
            try
            {
                ProgressTracker progressTracker = new ProgressTracker(directory);

                _ = MessageServer.SendMessageToClient(client, directoryName);
                _ = MessageServer.SendMessageToClient(client, GetDirectorySize(directory).ToString());
                _ = MessageServer.SendMessageToClient(client, Directory.GetDirectories(directory).Length.ToString());
                _ = MessageServer.SendMessageToClient(client, directory.Length.ToString());

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    string subDirectoryName = Path.GetFileName(subDirectory);
                    await Console.Out.WriteLineAsync($"Отправлен подкаталог {subDirectoryName}");
                    await SendDirectoryAsync(client, subDirectory, subDirectoryName);
                }

                foreach (string fileName in Directory.GetFiles(directory))
                {
                    await SendFileAsync(client, fileName, Path.GetFileName(fileName), progressTracker);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                Console.ResetColor();
            }
        }
        public async Task SendFileAsync(TcpClient client, string file, string fileName, ProgressTracker progressTracker)
        {
            try
            {
                if (File.Exists(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    long fileSize = fileInfo.Length;
                    long totalBytesSent = 0;
                    int bytesSent;

                    _ = MessageServer.SendMessageToClient(client, fileName);
                    _ = MessageServer.SendMessageToClient(client, fileSize.ToString());

                    if (fileSize > 250 * 1024 * 1024)
                    {
                        using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            while (totalBytesSent < fileSize)
                            {
                                byte[] buffer = new byte[150 * 1024 * 1024];
                                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                                totalBytesSent += bytesRead;

                                await client.GetStream().WriteAsync(buffer, 0, bytesRead);
                                bytesSent = bytesRead;

                               await Task.Factory.StartNew(() => progressTracker.ProgressRecive(totalBytesSent));
                            }
                        }
                    }
                    else
                    {
                        using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[fileSize];
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await client.GetStream().WriteAsync(buffer, 0, bytesRead);
                                totalBytesSent += bytesRead;

                                await Task.Factory.StartNew(() => progressTracker.ProgressRecive(totalBytesSent));
                            }
                        }
                    }

                    Console.WriteLine($"\nФайл отправлен клиенту");
                }
                else
                {
                    await Console.Out.WriteLineAsync("Файл не найден по указанному пути");
                }
            }
            catch (Exception ex) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
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