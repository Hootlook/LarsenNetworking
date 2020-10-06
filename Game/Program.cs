using LarsenNetworking;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Networker networker = new Networker();

            Command.Register(new Command[] {
                new ConnectionMessage(),
                new PrintMessage("NONE")
            });

            Console.WriteLine(Utils.label);
            Console.WriteLine("Welcome to LarsenNetworking !\n");

            Console.WriteLine("Please chose what to do:\n");
            Console.WriteLine("1) Host a Server");
            Console.WriteLine("2) Connect to a Server");

            int.TryParse(Console.ReadLine(), out int input);

            Console.WriteLine("\nTickRate in ms ?");
            int.TryParse(Console.ReadLine(), out int tick);
            networker.TickRate = tick;

            try
            {
                switch (input)
                {
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Clear();
                        networker.Host();
                        Console.WriteLine("Server Started !\n");

                        Task.Run(() =>
                        {
                            while (true)
                                lock(networker.Clients)
                                    foreach (var client in networker.Clients)
                                        client.Value?.ToString();
                        });

                        break;

                    case 2:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Clear();
                        Console.WriteLine(networker.Connect() ?
                            "Connection established !\n" :
                            "Connection failed...\n");

                        while (true)
                            networker.Send(new PrintMessage(Console.ReadLine()), new Random().Next(1, 3) == 1);

                    default:
                        goto case 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }

        [CmdType(SendingMethod.Reliable)]
        public class PrintMessage : Command
        {
            [CmdField]
            public string Message;

            public PrintMessage(string message)
            {
                Message = message;
            }

            public override void Execute()
            {
                Console.WriteLine(Message);
            }
        }

        public class ConnectionMessage : Command
        {
            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }
    }
}
