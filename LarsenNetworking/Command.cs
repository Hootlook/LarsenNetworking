using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LarsenNetworking
{
    public enum Method
    {
        Unreliable,
        Reliable,
        Ordered
    }

    public abstract class Command
    {
        public static List<Command> List { get; set; }
        public static Dictionary<Type, int> Lookup { get; set; }
        public static Queue<Command> toSendUnreliably = new Queue<Command>();
        public static Queue<Command> toSendReliably = new Queue<Command>();
        public static Queue<Command> toSendOrdered = new Queue<Command>();
        public int Id { get; protected set; }
        private FieldInfo[] Fields { get; set; }

        protected abstract void Action();

        public Command(int id, FieldInfo[] field) 
        {
            Id = id;
            Fields = field;
        }

        public Command()
        {
            Command command = List[Lookup[GetType()]];
            Id = command.Id;
            Fields = command.Fields;
        }

        public static void Initialize()
        {
            //List = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            //    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Command)))
            //    .Select(t => (Command)Activator.CreateInstance(t)).ToList();

            Lookup = new Dictionary<Type, int>();
            List = new List<Command>();

            foreach (Type type in
                    AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Command))))
            {
                Command command = (Command)Activator.CreateInstance(type, List.Count, type.GetFields());
                Lookup.Add(type, command.Id);
                List.Add(command);
            }
        }

        public void Send(Method sending)
        {
            Id = Lookup[GetType()];

            switch (sending)
            {
                case Method.Reliable:
                    lock (toSendReliably)
                        toSendReliably.Enqueue(this);
                    break;
                case Method.Ordered:
                    lock (toSendOrdered)
                        toSendOrdered.Enqueue(this);
                    break;
                case Method.Unreliable:
                    lock (toSendUnreliably)
                        toSendUnreliably.Enqueue(this);
                    break;
                default:
                    break;
            }
        }
    }

    public class PlayerMove : Command
    {
        protected override void Action()
        {
            throw new NotImplementedException();
        }
    }
}
