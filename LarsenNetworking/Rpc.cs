using System;
using System.Collections.Generic;
using System.Reflection;

namespace LarsenNetworking
{
    public class Rpc
    {
        public Enum Name { get; set; }
        public Delegate Method { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public object[] Values { get; set; }

        public enum SendingMethod
        {
            Reliable,
            Unreliable
        }
        public static Dictionary<Enum, int> lookup = new Dictionary<Enum, int>();
        public static List<Rpc> list = new List<Rpc>();
        public static Queue<Rpc> toSend = new Queue<Rpc>();

        public static void Register<T>(Enum name, T method)
            where T : Delegate
        {
            if (!lookup.ContainsKey(name))
            {
                lookup.Add(name, list.Count);
                list.Add(new Rpc()
                {
                    Name = name,
                    Method = method,
                    Parameters = method.Method.GetParameters()
                });
            }
            else
                throw new Exception("Already registered");
        }

        public static void Call(Enum rpcName, SendingMethod sending)
        {
            //TODO
            lock (toSend)
                toSend.Enqueue(list[lookup[rpcName]]);
        }

        public Type[] GetParameters() 
        {
            Type[] types = new Type[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++)
                types[i] = Parameters[i].ParameterType;

            return types;
        }
    }
}
