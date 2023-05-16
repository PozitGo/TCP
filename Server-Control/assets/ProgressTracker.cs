using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Channels;

namespace Server_Control.assets
{
    public class ProgressTracker
    {
        public long TotalBytes;
        private long BytesSend;
        public static int Progress;

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
        public void GetProgress(long BytesSend, ProgressPerforms performs)
        {
            this.BytesSend += BytesSend;
            Progress = (int)Math.Round(this.BytesSend / (double)TotalBytes * 100, 0);
            Console.ForegroundColor = ConsoleColor.Blue;

            if (performs == ProgressPerforms.Send)
            {
                Console.Write($"\rПрогресс отправки - {Progress}%".PadRight(Console.WindowWidth));
            }
            else
            {
                Console.Write($"\rПрогресс получения - {Progress}%".PadRight(Console.WindowWidth));
            }

            Console.ResetColor();

            if (Progress is 100)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                if (performs == ProgressPerforms.Send)
                {
                    Console.WriteLine("\nВсе файлы успешно отправлены.");
                }
                else
                {
                    Console.WriteLine("\nВсе файлы успешно получены.");
                }

                Console.ResetColor();
            }
        }

    }

    public enum ProgressPerforms
    {
        Send,
        Recive
    }
}
