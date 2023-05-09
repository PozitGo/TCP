using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Server_Control
{
    public class ProgressTracker
    {
        public long TotalBytes;
        private long BytesSend;
        public static int Progress;
        private bool isFirstIteration = true;

        public ProgressTracker(string Path = null)
        {
            TotalBytes = GetDirectoryAndFileSize(Path);

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

        public void ProgressRecive(long BytesSend)
        {
            this.BytesSend += BytesSend;
            Progress = (int)(Math.Round((TotalBytes / (double)this.BytesSend) * 100, 0));

            if (isFirstIteration)
            {
                Console.WriteLine($"Прогресс Отправки/Получения - {Progress}%");
                isFirstIteration = false;
            }
            else
            {
                Console.WriteLine($"\rПрогресс Отправки/Получения - {Progress}%");
            }
        }
    }
}
