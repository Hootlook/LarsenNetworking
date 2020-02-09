using LarsenNetworking;
using System; 

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            NetEntity entity;
            int input;

        Start:
           
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
                    entity = new UDPServer(5);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Clear();
                    break;

                case 2:
                    entity = new UDPClient();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Clear();
                    break;

                default:
                    Console.Clear();
                    goto Start;
            }
            Utils.SlowWrite("YEAH");
            Console.Read();
        }
    }
}
