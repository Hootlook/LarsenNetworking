using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Rpc
    {
        public int Id { get; private set; }
        public bool Reliable { get; private set; }
        public string Name { get; set; }
        public Action<Packet> Action { get; set; }

        public Rpc(string name, Action<Packet> action)
        {
            Name = name;
            Action = action;
        }

        private static Dictionary<string, int> _commandLookUp = new Dictionary<string, int>();
        private static List<Rpc> _commands = new List<Rpc>();
        public static List<Rpc> pending = new List<Rpc>();
        public static Queue<Rpc> toSend = new Queue<Rpc>();

        public static void Register(Rpc rpc)
        {
            rpc.Id = _commands.Count + 1;
            _commandLookUp.Add(rpc.Name, _commands.Count);
            _commands.Add(rpc);
        }

        public static void Call(string rpcName, bool reliable)
        {
            int index = _commandLookUp[rpcName];
            Rpc rpc = _commands[index];

            rpc.Reliable = reliable;

            lock (toSend)
                toSend.Enqueue(rpc);
        }
    }
}
