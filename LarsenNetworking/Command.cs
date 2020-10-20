using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LarsenNetworking
{
    public abstract class Command
    {
        public static List<Command> List { get; set; }
        public static Dictionary<Type, int> Lookup { get; set; }
        public static bool Initialized { get; private set; }
        public SendingMethod Method { get; private set; }
        public FieldInfo[] Fields { get; set; }
        public int Size { get; private set; }
        public int Id { get; private set; }
        public ushort PacketId { get; set; }
        public ushort OrderId { get; set; }
        public DateTime SendTime { get; set; }

        public abstract void Execute();

        public Command Clone() => (Command)MemberwiseClone();

        public Command()
        {
            if (!Initialized) return;

            Command command = List[Lookup[GetType()]];

            Method = command.Method;
            Fields = command.Fields;
            Size = command.Size;
            Id = command.Id;
        }

        public static void Register(Command[] commandes)
        {
            Lookup = new Dictionary<Type, int>();
            List = new List<Command>();
            
            foreach (Command command in commandes)
            {
                Type type = command.GetType();
                Type attField = typeof(CmdFieldAttribute);
                Type attType = typeof(CmdTypeAttribute);

                command.Method = (type.GetCustomAttribute(attType, false) as CmdTypeAttribute)?.Method ?? SendingMethod.ReliableOrdered;
                command.Fields = type.GetFields().Where(i => i.GetCustomAttributes(attField, false).Length > 0).ToArray();
                command.Size = Packet.Empty.WriteCommand(command);
                command.Id = List.Count;

                Lookup.Add(type, command.Id);
                List.Add(command);
            }

            Initialized = true;
        }

        //public static void Initialize()
        //{
        //    Lookup = new Dictionary<Type, int>();
        //    List = new List<Command>();

        //    foreach (Type type in
        //            AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
        //            .Where(t => t.IsClass && !t.IsInterface && typeof(IMessage).IsAssignableFrom(t)))
        //    {
        //        IMessage command = (IMessage)Activator.CreateInstance(type);
        //        Command registry = new Command(command, List.Count, type.GetFields());
        //        Lookup.Add(type, registry.Id);
        //        List.Add(registry);
        //    }
        //}

        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        public class CmdTypeAttribute : Attribute
        {
            public SendingMethod Method { get; set; }
            public CmdTypeAttribute(SendingMethod method) => Method = method;
        }

        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
        public class CmdFieldAttribute : Attribute { }

        public enum SendingMethod
        {
            Unreliable,
            Reliable,
            ReliableOrdered
        }

        #region BaseCommands

        [CmdType(SendingMethod.ReliableOrdered)]
        public class ServerCommand : Command
        {
            public enum ServerEvent
            {
                RequestConnection,
                Connected,
                Disconnected,
                Refused
            }

            [CmdField]
            public ServerEvent state;

            public ServerCommand(ServerEvent state)
            {
                this.state = state;
            }

            public override void Execute()
            {
                Console.WriteLine(state);
            }
        }

        #endregion
    }
}