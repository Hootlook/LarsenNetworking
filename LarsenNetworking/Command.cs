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

    public interface IMessage
    {
        void Execute();
    }

    public class Command
    {
        public static List<Command> List { get; set; }
        public static Dictionary<Type, int> Lookup { get; set; }
        public static Queue<Command> toSendUnreliably = new Queue<Command>();
        public static Queue<Command> toSendReliably = new Queue<Command>();
        public static Queue<Command> toSendOrdered = new Queue<Command>();
        public IMessage Message { get; set; }
        public int Id { get; private set; }
        public FieldInfo[] Fields { get; set; }

        private Command(IMessage message, int id, FieldInfo[] fields)
        {
            Message = message;
            Fields = fields;
            Id = id;
        }

        public Command(IMessage message)
        {
            Command command = List[Lookup[message.GetType()]];

            Fields = command.Fields;
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

        public static void Send(IMessage message, Method sending)
        {
            Command command = List[Lookup[message.GetType()]];

            switch (sending)
            {
                case Method.Reliable:
                    lock (toSendReliably)
                        toSendReliably.Enqueue(command);
                    break;
                case Method.Ordered:
                    lock (toSendOrdered)
                        toSendOrdered.Enqueue(command);
                    break;
                case Method.Unreliable:
                    lock (toSendUnreliably)
                        toSendUnreliably.Enqueue(command);
                    break;
                default:
                    break;
            }
        }
    }

    //public class PrintMessage : IMessage
    //{
    //    public string message;
    //    public PrintMessage(string message)
    //    {
    //        this.message = message;
    //    }
    //    public void Execute()
    //    {
    //        Console.WriteLine(message);
    //    }
    //}

    public class PrintMessage : IMessage
    {
        public string sequence;
        public string ack;
        public string ackBits;

        public PrintMessage(string sequence, string ack, string ackBits)
        {
            this.sequence = sequence;
            this.ack = ack;
            this.ackBits = ackBits;
        }
        public void Execute()
        {
            Console.WriteLine("//////////////////// REMOTE //////////////////////");
            Console.WriteLine(
                $"Sequence : {sequence}\n" +
                $"Ack : {ack}\n" +
                $"AckBits : {ackBits}\n"
                );
            Console.WriteLine("////////////////////////////////////////////////");
        }
    }

    public class ConnectMessage : IMessage
    {
        public void Execute()
        {
            Command.Send(new ChallengeConnectMessage(Networker.CONNECT_MESSAGE), Method.Reliable);
        }
    }

    public class ChallengeConnectMessage : IMessage
    {
        public string connectMessage;

        public ChallengeConnectMessage(string message)
        {
            connectMessage = message;
        }

        public void Execute()
        {
            Command.Send(new CompleteConnectMessage(), Method.Reliable);
        }
    }

    public class CompleteConnectMessage : IMessage
    {
        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //public class MyClass
    //{
    //    public static List<MyClass> List { get; set; }
    //    public static Dictionary<Type, int> Lookup { get; set; }
    //    public static void Initialize()
    //    {
    //        Lookup = new Dictionary<Type, int>();
    //        List = new List<MyClass>();

    //        foreach (Type type in
    //                AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
    //                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MyClass))))
    //        {
    //            MyClass command = (MyClass)Activator.CreateInstance(type);
    //            MyClass.ChildA.
    //            Lookup.Add(type, List.Count);
    //            List.Add(command);
    //        }
    //}

    //public abstract class BaseClass<T> : MyClass where T : class
    //{
    //    public static int id;
    //}

    //public class ChildA : BaseClass<ChildA>
    //{
    //}

    //public class ChildB : BaseClass<ChildB>
    //{
    //}
}