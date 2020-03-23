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
        public string Name { get; set; }
        public Action Action { get; set; }

        public Rpc(string name, Action action)
        {
            Name = name;
            Action = action;
        }

        private static Dictionary<string, int> _commandLookUp = new Dictionary<string, int>();
        private static List<Rpc> _commands = new List<Rpc>();
        public static List<Rpc> pending = new List<Rpc>();

        public static void Register(Rpc rpc)
        {
            _commandLookUp.Add(rpc.Name, _commands.Count);
            _commands.Add(rpc);
            rpc.Id = _commands.Count + 1;
        }

        public static void Call(string commandName)
        {
            int index = _commandLookUp[commandName];
            Rpc command = _commands[index];

            lock (pending)
                pending.Add(command);
        }
    }
}
