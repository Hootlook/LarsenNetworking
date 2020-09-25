using LarsenNetworking;
using System;
using System.Collections.Generic;
using System.Threading;
using static LarsenNetworking.Connection;
using static LarsenNetworking.Networker;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Networker server = new Networker();

            Command.Register(new IMessage[] {
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
            server.TickRate = tick;

            try
            {
                switch (input)
                {
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Clear();
                        server.Host();
                        Console.WriteLine("Server Started !\n");
                        break;

                    case 2:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Clear();
                        Console.WriteLine(server.Connect() ?
                            "Connection established !\n" :
                            "Connection failed...\n");

                        while (true)
                            server.Send(new PrintMessage(Console.ReadLine()), new Random().Next(1, 3) == 1);

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

        public class ConnectionMessage : IMessage
        {
            public void Execute()
            {

            }
        }
    }
}
