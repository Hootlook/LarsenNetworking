using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace LarsenNetworking
{
    public abstract class Entity
    {
        public bool IsBound { get; set; }
        public bool IsServer { get { return this is IServer; } }
    }
}
