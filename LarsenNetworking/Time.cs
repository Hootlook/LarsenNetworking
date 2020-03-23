using System.Diagnostics;

namespace LarsenNetworking
{
    public class Time
    {
        public Stopwatch timer { get; private set; }
        public ulong TimeStep { get; set; }
        public Time()
        {
            timer = new Stopwatch();
            timer.Start();
        }
    }
}