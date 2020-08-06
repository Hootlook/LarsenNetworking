using System;
using System.Collections.Generic;
using System.Reflection;

namespace LarsenNetworking
{
    public interface IMessage
    {
        void Execute();
    }

    public class Command
    {
        public static List<Command> List { get; set; }
        public static Dictionary<Type, int> Lookup { get; set; }
        public FieldInfo[] Fields { get; set; }
        public IMessage Message { get; set; }
        public int Id { get; private set; }
        public int Size { get; set; }
        public ushort PacketId { get; set; }
        public DateTime SendTime { get; set; }

        private Command(IMessage message, int id, FieldInfo[] fields)
        {
            Message = message;
            Fields = fields;
            Id = id;

            Packet packet = Packet.Empty;

            Size = packet.WriteCommand(this);
        }

        public Command(IMessage message)
        {
            Command command = List[Lookup[message.GetType()]];

            Fields = command.Fields;
            Size = command.Size;
            Id = command.Id;
            
            Message = message;
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
        public static void Register(IMessage[] messages)
        {
            Lookup = new Dictionary<Type, int>();
            List = new List<Command>();

            foreach (IMessage message in messages)
            {
                Type messageType = message.GetType();
                Command command = new Command(message, List.Count, messageType.GetFields());
                Lookup.Add(messageType, command.Id);
                List.Add(command);
            }
        }
    }
}