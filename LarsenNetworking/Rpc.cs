using System;
using System.Collections.Generic;

namespace LarsenNetworking
{
    public class Rpc
    {
        public int Id { get; private set; }
        public int TimeStep { get; private set; }
        public bool Reliable { get; private set; }
        public Enum Label { get; set; }
        public Action<Packet> Action { get; set; }

        public Rpc(Action<Packet> action)
        {
            Action = action;
        }

        public static Dictionary<Enum, int> lookup = new Dictionary<Enum, int>();
        public static List<Rpc> list = new List<Rpc>();
        public static Queue<Rpc> toSend = new Queue<Rpc>();

        public static void Register(Enum key, Action<Packet> action)
        {
            if (!lookup.ContainsKey(key))
            {
                lookup.Add(key, list.Count);
                list.Add(new Rpc(action));
            }
            else
                throw new Exception("Already registered");
        }

        public static void Call(Enum key)
        {
            //TODO
            lock (toSend)
                toSend.Enqueue(list[lookup[key]]);
        }
    }
}
