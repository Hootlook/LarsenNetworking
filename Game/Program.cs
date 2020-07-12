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
            });

            Console.WriteLine(Utils.label);
            Utils.SlowWrite("Welcome to LarsenNetworking !\n");
            Console.ResetColor();

            Utils.SlowWrite("Please chose what to do:\n");
            Utils.SlowWrite("1) Host a Server");
            Utils.SlowWrite("2) Connect to a Server");

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
                        Utils.SlowWrite("Server Started !\n");
                        break;

                    case 2:
                        client = new Client();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Clear();
                        Utils.SlowWrite(client.Connect() ?
                            "Connection established !\n" :
                            "Connection failed...\n");
                        break;

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
    }
}
