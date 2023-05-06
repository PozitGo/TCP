﻿using System;
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
        public readonly Client client;

        public ReciveCommand(string SavePath, Client client)
        {
            this.SavePath = SavePath;
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
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
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
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await client.Connect();
            }
        }

        public async Task ReciveFileAsync(NetworkStream clientStream, string file)
        {
            try
            {
                long fileSize;

                BinaryReader reader = new BinaryReader(clientStream);
                fileSize = reader.ReadInt64();

                Console.WriteLine("Получен файл: {0}, размер: {1} байт(а).", file, fileSize);

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

                Console.WriteLine($"Файл {file} успешно сохранен на диск.\n");
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
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
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
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
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

                foreach (string fileName in Directory.GetFiles(directory))
                {
                    await SendFileAsync(clientStream, fileName, Path.GetFileName(fileName));
                }

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    string subDirectoryName = Path.GetFileName(subDirectory);
                    await SendDirectoryAsync(clientStream, subDirectory, subDirectoryName);
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
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await client.Connect();
            }
        }

        public async Task SendFileAsync(NetworkStream clientStream, string file, string fileName)
        {
            try
            {
                if (File.Exists(file))
                {
                    long fileSize;

                    BinaryWriter writer = new BinaryWriter(clientStream);

                    FileInfo fileInfo = new FileInfo(file);
                    fileSize = fileInfo.Length;
                    writer.Write(fileName);
                    writer.Write(fileInfo.Length);

                    Console.WriteLine($"Отправлен {fileName}, длина - {fileInfo.Length} байт(а)");


                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[fileSize];
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
                Console.WriteLine($"Ошибка" + ex.Message);
                await Console.Out.WriteLineAsync("\nСоединение с сервером потеряно, попытка переподключиться");
                await client.Connect();
            }
        }
    }
}