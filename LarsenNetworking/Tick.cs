using System.Diagnostics;

namespace LarsenNetworking
{
    public class Tick
    {
        private Stopwatch Timer { get; set; }
        private long LastTick { get; set; }

        private int _rate = 30;

        public int Rate
        {
            get { return 1000 / _rate; }
            set
            {
                if (value > 170)
                    _rate = 170;
                else if (value < 20)
                    _rate = 20;
                else
                    _rate = value;
            }
        }

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