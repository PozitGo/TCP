using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PublicClient
{
    public interface ICommand
    {
        Task ExecuteAsync(NetworkStream clientStream);
    }
    public class ReciveCommand : ICommand
    {
        public string SavePath;
        private string CurrentDirectory;
        private Client client;

        public ReciveCommand(string SavePath, Client client)
        {
            this.SavePath = SavePath;
            this.client = client;
            CurrentDirectory = SavePath;
        }

        public async Task ExecuteAsync(NetworkStream clientStream)
        {
            try
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
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
        }

        public async Task ReciveDirectoryAsync(NetworkStream clientStream, string directory)
        {
            try
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
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
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
                                await Console.Out.WriteAsync($"\nПрогресс получения файла {Path.GetFileName(file)}  - {Math.Round((totalBytesReceived / (double)fileSize) * 100, 0)}%");
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

                Console.WriteLine($"Файл {file} успешно сохранен на диск.");
            }
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
        }
    }

    public class SendCommand : ICommand
    {
        public readonly string SendPath;
        public readonly Client client;

        public SendCommand(string SendPath, Client client)
        {
            this.client = client;
            this.SendPath = SendPath;
        }

        public async Task ExecuteAsync(NetworkStream clientStream)
        {

            try
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
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
        }

        public async Task SendDirectoryAsync(NetworkStream clientStream, string directory, string directoryName)
        {
            try
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
            catch (SocketException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                Console.ResetColor();
                await client.Connect();
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

                                if (isFirstIteration)
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
            catch (SocketException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await client.Connect();
            }
            catch (IOException)
            {
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка " + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await client.Connect();
            }
        }

    }
}