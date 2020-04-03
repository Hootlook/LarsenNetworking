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

        public static Dictionary<string, int> commandLookUp = new Dictionary<string, int>();
        public static List<Rpc> commands = new List<Rpc>();
        public static List<Rpc> pending = new List<Rpc>();
        public static Queue<Rpc> toSend = new Queue<Rpc>();

        public static void Register(Rpc rpc)
        {
            rpc.Id = commands.Count + 1;
            commandLookUp.Add(rpc.Name, commands.Count);
            commands.Add(rpc);
        }

        public static void Call(string rpcName, bool reliable)
        {
            int index = commandLookUp[rpcName];
            Rpc rpc = commands[index];

            rpc.Reliable = reliable;

            lock (toSend)
                toSend.Enqueue(rpc);
        }
    }
}
