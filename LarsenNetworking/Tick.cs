using System.Diagnostics;

namespace LarsenNetworking
{
    public class Tick
    {
        private Stopwatch Timer { get; set; }
        private long LastTick { get; set; }
        public int Rate { get; set; } = 30;

        public Tick(Stopwatch timer) => Timer = timer;

        public bool TryTick()
        {
            if (Timer.ElapsedMilliseconds - LastTick <= (1000 / Rate))
                return false;

            LastTick = Timer.ElapsedMilliseconds;
            return true;
        }
    }
}