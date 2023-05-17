using System;
using System.IO;

namespace Server_Control.assets
{
    public class ProgressTracker
    {
        public long TotalBytes;
        private long BytesSend;
        public int Progress;

        public ProgressTracker(string Path)
        {
            TotalBytes = GetDirectoryAndFileSize(Path);
        }

        public ProgressTracker(long TotalBytes)
        {
            this.TotalBytes = TotalBytes;
        }

        public ProgressTracker()
        {

        }

        private long GetDirectoryAndFileSize(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                long size = 0;

                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += fileInfo.Length;
                }

                return size;
            }
            else
            {
                FileInfo file = new FileInfo(path);
                return file.Length;
            }
        }
        public void GetProgress(long bytesProcessed, ProgressPerforms performs)
        {
            if (this.Progress < 100)
            {
                this.BytesSend += bytesProcessed;
                this.Progress = (int)Math.Round(this.BytesSend / (double)TotalBytes * 100, 0);

                switch (performs)
                {
                    case ProgressPerforms.Send:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\rПрогресс отправки - {this.Progress}%".PadRight(Console.WindowWidth));
                        Console.ResetColor();

                        if (this.Progress is 100)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nВсе файлы успешно отправлены.");
                            Console.ResetColor();
                        }

                        break;

                    case ProgressPerforms.Recive:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\rПрогресс получения - {this.Progress}%".PadRight(Console.WindowWidth));
                        Console.ResetColor();

                        if (this.Progress is 100)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nВсе файлы успешно получены.");
                            Console.ResetColor();
                        }

                        break;
                }
            }
        }
    }

    public enum ProgressPerforms
    {
        Send,
        Recive
    }
}
