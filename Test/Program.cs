using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        //[DllImport("user32.dll")]
        //public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        //const int WM_APPCOMMAND = 0x319;
        //const int APPCOMMAND_VOLUME_UP = 0xA0000;
        //static void IncreaseVolume()
        //{
        //    IntPtr hWnd = IntPtr.Zero;
        //    IntPtr wParam = (IntPtr)0;
        //    IntPtr lParam = (IntPtr)APPCOMMAND_VOLUME_UP;

        //    SendMessageW(hWnd, WM_APPCOMMAND, wParam, lParam);
        //}

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 0x0014;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;
        static void Main(string[] args)
        {
            //string audioFilePath = @"C:\Users\Exper\Music\24121--.mp3";

            //try
            //{
            //    var deviceEnumerator = new MMDeviceEnumerator();
            //    var defaultRenderDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            //    // Увеличение громкости на 10%
            //    float volumeIncrement = 0.01f;
            //    float targetVolume = 1.0f;
            //    IncreaseVolume();
            //    Task.Factory.StartNew(() => {
            //        while (true)
            //        {
            //            if(defaultRenderDevice.AudioEndpointVolume.MasterVolumeLevelScalar < targetVolume)
            //            {
            //                defaultRenderDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;
            //            }
            //        }
            //    });

            //    using (var audioFile = new AudioFileReader(audioFilePath))
            //    using (var outputDevice = new WaveOutEvent())
            //    {
            //        outputDevice.Init(audioFile);
            //        outputDevice.Play();

            //        Console.WriteLine("Воспроизведение началось. Нажмите Enter для остановки.");
            //        Console.ReadLine();

            //        outputDevice.Stop();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Ошибка при проигрывании трека: " + ex.Message);
            //}

            //Console.ReadLine();

            try
            {
                string imagePath = @"C:\Users\Exper\Desktop\image6.png";
                SetWallpaper(imagePath);
                Console.WriteLine("Обои успешно установлены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при установке обоев: " + ex.Message);
            }

            Console.ReadLine();
        }

        static void SetWallpaper(string imagePath)
        {
            int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            if (result == 0)
            {
                throw new Exception("Не удалось установить обои.");
            }
        }
    }
}
