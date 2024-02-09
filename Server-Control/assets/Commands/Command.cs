using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

                string Name = Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString()));

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

                    progressTracker.TotalBytes = Convert.ToInt64(Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString())));

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
                Console.WriteLine($"\nОшибка 1 " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveDirectoryAsync(TcpClient client, string directory)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());

                int CountDirectories = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString())));
                int CountFiles = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString())));

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
                Console.WriteLine($"\nОшибка 2 " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task ReciveFileAsync(TcpClient client, string file, ProgressTracker progressTracker, bool IsFullFile = false)
        {
            try
            {
                BinaryReader reader = new BinaryReader(client.GetStream());
                long fileSize = Convert.ToInt64(Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString())));

                if (IsFullFile)
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
                Console.WriteLine($"\nОшибка 3 " + ex.Message);
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
                Console.WriteLine($"\nОшибка 4 " + ex.Message);
                Console.ResetColor();
            }
        }

        public async Task SendDirectoryAsync(TcpClient client, string directory, string directoryName)
        {
            try
            {
                BinaryWriter writer = new BinaryWriter(client.GetStream());

                long SizeDirectory = GetDirectorySize(directory);
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(directoryName)));
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SizeDirectory.ToString())));
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((Directory.GetDirectories(directory).Length.ToString()))));
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Directory.GetFiles(directory).Length.ToString())));

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
                Console.WriteLine($"\nОшибка 5 " + ex.Message);
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

                    writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileName)));
                    writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileSize.ToString())));

                    if (IsFullFile)
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
                Console.WriteLine($"\nОшибка 6 " + ex.Message);
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

    class ProcessCommand : ICommand
    {
        private readonly string Command;
        private readonly string Arguments;

        public ProcessCommand(string Command, string Arguments)
        {
            this.Command = Command;
            this.Arguments = Arguments;
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            switch (this.Command)
            {
                case "$ProcessKill":

                    await KillProcess(client, Arguments);

                    break;
                case "$GetProcess":

                    await GetProcess(client);

                    break;
                case "$StartProcess":

                    await StartProcess(client, Arguments);

                    break;
                case "$InfoProcess":

                    await InfoProcess(client, Arguments);

                    break;
                default:
                    Console.WriteLine($"Не существует комманды {Command} для работы с процессами");
                    break;
            }
        }

        private async Task GetProcess(TcpClient client)
        {
            await MessageServer.SendMessageToClient(client, Command);

            BinaryReader binaryReader = new BinaryReader(client.GetStream());
            int CountProcess = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(binaryReader.ReadString())));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Список запущенных процессов у клиента:");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            for (int i = 0; i < CountProcess; i++)
            {
                SerializableProcess process = ProcessSerializer.DeserializeProcess(binaryReader.ReadString());
                Console.WriteLine($"Name: {process.ProcessName}, Id: {process.Id}, StartTime: {process.StartTime}, UsesRAM: {process.UsesRAM} Мб, FilePath: {process.FilePath}");
            }

            Console.ResetColor();
        }

        private async Task KillProcess(TcpClient client, string Id)
        {
            await MessageServer.SendMessageToClient(client, $"{Command} {Id}");

            if (await MessageServer.ReadClientMessage(client) is "$success")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Процесс с ID: {Id} успешно закрыт");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка закрытия процесса");
                Console.ResetColor();
            }
        }

        private async Task StartProcess(TcpClient client, string Arguments)
        {
            await MessageServer.SendMessageToClient(client, $"{Command} {Arguments}");

            if (await MessageServer.ReadClientMessage(client) is "$success")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Процесс успешно запущен");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка запуска процесса");
                Console.ResetColor();
            }
        }

        private async Task InfoProcess(TcpClient client, string Id)
        {
            await MessageServer.SendMessageToClient(client, $"{Command} {Id}");

            BinaryReader binaryReader = new BinaryReader(client.GetStream());

            if (int.TryParse(Id, out int value))
            {
                SerializableProcess process = ProcessSerializer.DeserializeProcess(binaryReader.ReadString());

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"\nName: {process.ProcessName}\nId: {process.Id}\nStartTime: {process.StartTime}\nUsesRAM: {process.UsesRAM} Мб\nFilePath: {process.FilePath}");
                Console.ResetColor();
            }
            else
            {

                int CountProcess = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(binaryReader.ReadString())));

                for (int i = 0; i < CountProcess; i++)
                {
                    SerializableProcess process = ProcessSerializer.DeserializeProcess(binaryReader.ReadString());

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\nName: {process.ProcessName}\nId: {process.Id}\nStartTime: {process.StartTime}\nUsesRAM: {process.UsesRAM} Мб\nFilePath: {process.FilePath}");
                    Console.ResetColor();
                }
            }
        }
    }

    class MusicCommand : ICommand
    {
        public async Task ExecuteAsync(TcpClient client)
        {
            try
            {
                if (MessageServer.ReadClientMessage(client).Result is "$success")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Трек успешно запущен");
                    Console.ResetColor();

                    string Command = default;

                    do
                    {
                        Console.WriteLine($"Введите команду $Pstop чтобы остановить проигрывание");
                        Command = Console.ReadLine();

                    } while (!(Command is "$Pstop"));

                    await MessageServer.SendMessageToClient(client, Command);

                    if (MessageServer.ReadClientMessage(client).Result is "$success")
                    {

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Трек успешно остановлен");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка остановки трека");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка запуска трека");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при проигрывании трека: " + ex.Message);
            }
        }
    }

    class WallpaperCommand : ICommand
    {
        public Task ExecuteAsync(TcpClient client)
        {
            try
            {
                if (MessageServer.ReadClientMessage(client).Result is "$success")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Обои успешно сменены");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Ошибка установки обоев");
                    Console.ResetColor();
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Ошибка установки обоев");
                Console.ResetColor();
            }

            return Task.CompletedTask;
        }
    }
}