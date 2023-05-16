using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PublicServer
{
    public interface ICommand
    {
        Task ExecuteAsync(NetworkStream clientStream);
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

        public ReciveCommand()
        {
            
        }

        public async Task ExecuteAsync(NetworkStream clientStream)
        {
            string Name;

            BinaryReader reader = new BinaryReader(clientStream);
            Name = reader.ReadString();

            if (Name.Contains("."))
            {
                await ReciveFileAsync(clientStream, Path.Combine(CurrentDirectory, Name));
            }
            else
            {
                await ReciveDirectoryAsync(clientStream, Name);
            }
        }

        public async Task ReciveDirectoryAsync(NetworkStream clientStream, string directory)
        {
            BinaryReader reader = new BinaryReader(clientStream);

            int CountFiles = reader.ReadInt32();
            int CountDirectories = reader.ReadInt32();

            CurrentDirectory = Path.Combine(CurrentDirectory, directory);
            await Console.Out.WriteLineAsync($"Получен каталог {directory}");
            Directory.CreateDirectory(CurrentDirectory);

            for (int i = 0; i < CountDirectories; i++)
            {
                await ExecuteAsync(clientStream);
            }

            for (int i = 0; i < CountFiles; i++)
            {
                await ExecuteAsync(clientStream);
            }

            CurrentDirectory = Directory.GetParent(CurrentDirectory).FullName;
        }

        public async Task ReciveFileAsync(NetworkStream clientStream, string file)
        {
            try
            {
                BinaryReader reader = new BinaryReader(clientStream);
                long fileSize = reader.ReadInt64();

                if (fileSize > 250 * 1024 * 1024)
                {
                    using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[150 * 1024 * 1024];
                        long totalBytesReceived = 0;
                        int bytesRead;
                        bool isFirstIteration = true;

                        while (totalBytesReceived < fileSize)
                        {
                            int bytesToRead = (int)Math.Min(fileSize - totalBytesReceived, buffer.Length);
                            bytesRead = await clientStream.ReadAsync(buffer, 0, bytesToRead);
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesReceived += bytesRead;

                            if (isFirstIteration)
                            {
                                await Console.Out.WriteAsync($"\nПрогресс получения файла {Path.GetFileName(file)} - {Math.Round((totalBytesReceived / (double)fileSize) * 100, 0)}%");
                                isFirstIteration = false;
                            }
                            else
                            {
                                await Console.Out.WriteAsync($"\rПрогресс получения файла {Path.GetFileName(file)} - {Math.Round((totalBytesReceived / (double)fileSize) * 100, 0)}%");
                            }
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
                            bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                        }
                    }
                }


                Console.WriteLine($"\nФайл {file} успешно сохранен на диск.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОшибка " + ex.Message);
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

        public SendCommand()
        {
            
        }

        public async Task ExecuteAsync(NetworkStream clientStream)
        {
            if (File.Exists(SendPath))
            {
                await SendFileAsync(clientStream, SendPath, Path.GetFileName(SendPath));
            }
            else
            {
                await SendDirectoryAsync(clientStream, SendPath, Path.GetFileName(SendPath));
            }
        }

        public async Task SendDirectoryAsync(NetworkStream clientStream, string directory, string directoryName)
        {
            BinaryWriter writer = new BinaryWriter(clientStream);

            writer.Write(directoryName);
            writer.Write(Directory.GetDirectories(directory).Length);
            writer.Write(Directory.GetFiles(directory).Length);


            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                string subDirectoryName = Path.GetFileName(subDirectory);
                await Console.Out.WriteLineAsync($"Отправлен подкаталог {subDirectoryName}");
                await SendDirectoryAsync(clientStream, subDirectory, subDirectoryName);
            }

            foreach (string fileName in Directory.GetFiles(directory))
            {
                await SendFileAsync(clientStream, fileName, Path.GetFileName(fileName));
            }
        }

        public async Task SendFileAsync(NetworkStream clientStream, string file, string fileName)
        {
            try
            {
                if (File.Exists(file))
                {
                    BinaryWriter writer = new BinaryWriter(clientStream);

                    FileInfo fileInfo = new FileInfo(file);
                    long fileSize = fileInfo.Length;
                    long totalBytesSent = 0;
                    int bytesSent;

                    writer.Write(fileName);
                    writer.Write(fileSize);

                    if (fileSize > 250 * 1024 * 1024)
                    {
                        using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            bool isFirstIteration = true;
                            while (totalBytesSent < fileSize)
                            {
                                byte[] buffer = new byte[150 * 1024 * 1024];
                                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                                totalBytesSent += bytesRead;

                                await clientStream.WriteAsync(buffer, 0, bytesRead);
                                bytesSent = bytesRead;

                                if(isFirstIteration) 
                                {
                                    await Console.Out.WriteAsync($"\nПрогресс отправки файла {Path.GetFileName(file)} - {Math.Round((totalBytesSent / (double)fileSize) * 100, 0)}%");
                                    isFirstIteration = false;
                                }
                                else
                                {
                                    await Console.Out.WriteAsync($"\rПрогресс отправки файла {Path.GetFileName(file)} - {Math.Round((totalBytesSent / (double)fileSize) * 100, 0)}%");
                                }

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
                                await clientStream.WriteAsync(buffer, 0, bytesRead);
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
                Console.WriteLine($"\nОшибка " + ex.Message);
                Console.ResetColor();
            }
        }
    }


    public class DeleteReciveCommand : ICommand
    {
        public readonly string RecivePath;

        public DeleteReciveCommand(string RecivePath)
        {
            this.RecivePath = RecivePath;
        }

        public DeleteReciveCommand()
        {
            
        }

        public Task ExecuteAsync(NetworkStream clientStream)
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
