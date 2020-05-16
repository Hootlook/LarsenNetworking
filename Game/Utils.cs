using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    public static class Utils
    {
        public static string label = "  _                                _   _      _                      _    _             \n" +
                                     " | |    __ _ _ __ ___  ___ _ __   | \\ | | ___| |___      _____  _ __| | _(_)_ __   __ _ \n" +
                                     " | |   / _` | '__/ __|/ _ \\ '_ \\  |  \\| |/ _ \\ __\\ \\ /\\ / / _ \\| '__| |/ / | '_ \\ / _` |\n" +
                                     " | |__| (_| | |  \\__ \\  __/ | | | | |\\  |  __/ |_ \\ V  V / (_) | |  |   <| | | | | (_| |\n" +
                                     " |_____\\__,_|_|  |___/\\___|_| |_| |_| \\_|\\___|\\__| \\_/\\_/ \\___/|_|  |_|\\_\\_|_| |_|\\__, |\n" +
                                     "                                                                                  |___/ \n";
        public static void SlowWrite(string text)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(5);
            }
            Console.WriteLine();
        }

        public static void GameOver()
        {
            int speed = 500;
            Console.Beep(850, speed);
            Console.Beep(620, speed);
            Console.Beep(650, speed);
            Console.Beep(500, speed);
            Console.Beep(550, speed);
            Console.Beep(420, speed);
            Console.Beep(470, speed);
            Console.Beep(490, speed);
            Console.Beep(630, speed);
            Console.Beep(420, speed);
            Console.Beep(470, speed);
            Console.Beep(420, speed);
            Console.Beep(370, speed);
            Console.Beep(285, speed);
            Console.Beep(320, speed);
            Console.Beep(210, speed);
        }
    }
}
