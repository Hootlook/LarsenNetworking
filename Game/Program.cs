using LarsenNetworking;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static LarsenNetworking.Command;
using static LarsenNetworking.Networker;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Client client = new Client();

            Command.Register(new Command[] {
                //new StateMessage(StateMessage.ConnectionState.Connected),
                new ServerCommand(ServerCommand.ServerEvent.Refused),
                new PrintMessage("NONE"),
            });

            Console.WriteLine(Utils.label);
            Console.WriteLine("Welcome to LarsenNetworking !\n");

            Console.WriteLine("Please chose what to do:\n");
            Console.WriteLine("1) Host a Server");
            Console.WriteLine("2) Connect to a Server");

            int.TryParse(Console.ReadLine(), out int input);

            Console.WriteLine("\nTickRate in ms ?");
            int.TryParse(Console.ReadLine(), out int tick);
            client.Tick.Rate = tick;
            server.Tick.Rate = tick;

            try
            {
                switch (input)
                {
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Clear();
                        server.Host();
                        Console.WriteLine("Server Started !\n");

                        while (true)
                            server.Update();

                    case 2:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Clear();
                        Console.WriteLine(client.Connect() ?
                            "Connection established !\n" :
                            "Connection failed...\n");

                        //int lastSeq = 0;
                        //var t = new System.Timers.Timer(1000);
                        //t.Elapsed += (o, e) =>
                        //{
                        //    Console.WriteLine(networker.Server.Sequence - lastSeq);
                        //    lastSeq = networker.Server.Sequence;
                        //};
                        //t.Start();

                        while (true)
                        {
                            //Thread.Sleep(100);
                            //Console.SetCursorPosition(0, 1);
                            //Console.Clear();
                            //Console.WriteLine($"Ping : {client.Server.Ping}");
                            //server.Send(new StateMessage(StateMessage.ConnectionState.Disconnected), new Random().Next(1, 3) == 1);
                            //server.Send(new ServerCommand(ServerCommand.ServerEvent.RequestConnection), new Random().Next(1, 3) == 1);
                            client.Send(new PrintMessage(Console.ReadLine()), new Random().Next(1, 3) == 1);
                        }

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

        [CmdType(SendingMethod.ReliableOrdered)]
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
    }
}
