using LarsenNetworking;
using System;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = null;
            Client client = null;

            Command.Register(new IMessage[] {
                new ConnectionMessage(),
                new PrintMessage("NONE")
            });

            Console.WriteLine(Utils.label);
            Console.WriteLine("Welcome to LarsenNetworking !\n");
            Console.ResetColor();

            Console.WriteLine("Please chose what to do:\n");
            Console.WriteLine("1) Host a Server");
            Console.WriteLine("2) Connect to a Server");

            int.TryParse(Console.ReadLine(), out int input);
            
            try
            {
                switch (input)
                {
                    case 1:
                        server = new Server(5);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Clear();
                        server.Run();
                        Console.WriteLine("Server Started !\n");
                        break;

                    case 2:
                        client = new Client();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Clear();
                        Console.WriteLine(client.Connect() ?
                            "Connection established !\n" :
                            "Connection failed...\n");

                        while (true)
                            client.Send(new PrintMessage(Console.ReadLine()));

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

        class PrintMessage : IMessage
        {
            public string _message;

            public PrintMessage(string message)
            {
                _message = message;
            }

            public void Execute()
            {
                Console.WriteLine(_message);
            }
        }
    }
}
