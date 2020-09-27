using System;
using System.Collections.Generic;
using System.Reflection;

namespace LarsenNetworking
{
    public abstract class Command
    {
        public static List<Command> List { get; set; }
        public static Dictionary<Type, int> Lookup { get; set; }
        public static bool Initialized { get; private set; }
        public FieldInfo[] Fields { get; set; }
        public int Id { get; private set; }
        public int Size { get; private set; }
        public ushort PacketId { get; set; }
        public DateTime SendTime { get; set; }

        public abstract void Execute();

        public Command Clone() => (Command)MemberwiseClone();

        public Command()
        {
            if (!Initialized) return;

            Command command = List[Lookup[GetType()]];

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
                Type commandType = command.GetType();

                command.Id = List.Count;
                command.Fields = commandType.GetFields();
                command.Size = Packet.Empty.WriteCommand(command);

                Lookup.Add(commandType, command.Id);
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
    }
}