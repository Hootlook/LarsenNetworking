using LarsenNetworking;
using System;
using System.Threading;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Networker entity;
            int input;
           
            Console.WriteLine(Utils.label);
            Utils.SlowWrite("Welcome to LarsenNetworking !\n");
            Console.ResetColor();

            Utils.SlowWrite("Please chose what to do:\n");
            Utils.SlowWrite("1) Host a Server");
            Utils.SlowWrite("2) Connect to a Server");

            int.TryParse(Console.ReadLine(), out input);
            
            switch (input)
            {
                case 1:
                    entity = new Server(5);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Clear();
                    break;

                case 2:
                    entity = new Client();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Clear();
                    break;

                default:
                    entity = new Server(5);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Clear();
                    break;
            }

            try
            {
                if (entity.IsServer)
                {
                    ((Server)entity).Run();
                    Utils.SlowWrite("Server Started !\n");
                }
                else
                {
                    ((Client)entity).Connect();
                    Utils.SlowWrite("Client Started !\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
