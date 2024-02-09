using System;
using System.Collections.Generic;
using System.Text;

namespace Server_Control.assets
{
    public static class Results
    {
        public static void Success()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Команда прошла валидацию");
            Console.ResetColor();
        }
    }
}
