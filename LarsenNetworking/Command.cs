using System;
using System.Collections.Generic;
using System.IO;
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
        public int Size { get; private set; }
        public int Id { get; private set; }
        public ushort PacketId { get; set; }
        public ushort OrderId { get; set; }
        public DateTime SendTime { get; set; }

        public abstract void Action();
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);

        public Command Clone() => (Command)MemberwiseClone();

        public Command()
        {
            if (!Initialized) return;

            Command command = List[Lookup[GetType()]];

            Method = command.Method;
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
                Type attType = typeof(CmdTypeAttribute);

                command.Method = (type.GetCustomAttribute(attType, false) as CmdTypeAttribute)?.Method ?? SendingMethod.ReliableOrdered;
                command.Size = command.GetBytes().Length;
                command.Id = List.Count;

                Lookup.Add(type, command.Id);
                List.Add(command);
            }

            Initialized = true;
        }

        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Id);

                if (Method != SendingMethod.Reliable)
                    writer.Write(OrderId);

                Serialize(writer);

                return stream.ToArray();
            }
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

        public static NetEvent GetEvent(Packet packet)
        {
            return !(TryUnpack(packet.Data.ToArray()) is NetEventCommand netEvent) ? NetEvent.None : netEvent.State;
        }

        public static Command TryUnpack(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    Command command = List[reader.ReadInt32()].Clone();

                    if (command.Method != SendingMethod.Reliable)
                        command.OrderId = reader.ReadUInt16();

                    command.Deserialize(reader);

                    return command;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        #region Attributes
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        public class CmdTypeAttribute : Attribute
        {
            public SendingMethod Method { get; set; }
            public CmdTypeAttribute(SendingMethod method) => Method = method;
        }

        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
        public class CmdFieldAttribute : Attribute { }
        #endregion

        public enum SendingMethod
        {
            Unreliable,
            Reliable,
            ReliableOrdered
        }
    }

    #region BaseCommands
    public enum NetEvent
    {
        None,
        ConnectionRequest,
        Connected,
        Disconnected,
        ServerFull,
        Accepted
    }

    public class ConnectionRequest : Command
    {
        public int ClientSalt { get; set; }
        public int ChallengeSalt { get; set; }

        public ConnectionRequest(int clientSalt)
        {
            ClientSalt = clientSalt;
        }
        public ConnectionRequest(int clientSalt, int challengeSalt)
        {
            ClientSalt = clientSalt;
            ChallengeSalt = challengeSalt;
        }

        public override void Action()
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            ClientSalt = reader.ReadInt32();
            ChallengeSalt = reader.ReadInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientSalt);
            writer.Write(ChallengeSalt);
        }
    }

    [CmdType(SendingMethod.ReliableOrdered)]
    public class NetEventCommand : Command
    {
        public NetEvent State { get; set; }
        public byte[] Data { get; set; }

        public NetEventCommand(NetEvent state, byte[] data = null)
        {
            State = state;
            Data = data;
        }

        public override void Action()
        {
            Console.WriteLine(State);
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)State);
        }

        public override void Deserialize(BinaryReader reader)
        {
            State = (NetEvent)reader.ReadByte();
        }
    }

    #endregion
}