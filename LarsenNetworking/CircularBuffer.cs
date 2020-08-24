using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    class CircularBuffer<T>
    {
        public int MaxSize { get; set; }
        private int Head { get; set; }
        private int Tail { get; set; }
        private T[] Buffer { get; set; }

        public CircularBuffer(int maxSize)
        {
            MaxSize = maxSize;
        }
    }
}
