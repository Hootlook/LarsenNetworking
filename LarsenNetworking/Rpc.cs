using System;
using System.Collections.Generic;
using System.Reflection;

namespace LarsenNetworking
{
    public class Rpc
    {
        public int Id { get; private set; }
        public int TimeStep { get; private set; }
        public bool Reliable { get; private set; }
        public Enum Label { get; set; }
        public Delegate Method { get; set; }
        public ParameterInfo[] Parameters { get; set; }

        public static Dictionary<Enum, int> lookup = new Dictionary<Enum, int>();
        public static List<Rpc> list = new List<Rpc>();
        public static Queue<Rpc> toSend = new Queue<Rpc>();

        //public static void Register(Enum methodName, Type methodLocation)
        //{

        //}

        public static void Register<T>(Enum methodName, T method)
            where T : Delegate
        {
            if (!lookup.ContainsKey(methodName))
            {
                lookup.Add(methodName, list.Count);
                list.Add(new Rpc()
                {
                    Label = methodName,
                    Method = method,
                    Parameters = method.Method.GetParameters()
                });
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
