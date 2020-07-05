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
                new Client.ConnectionMessage(),
                new PrintMessage("")
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

            //if (client != null)
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        Packet packet = Packet.Empty;
            //        packet.WriteCommand(new Command(new PrintMessage(i.ToString())));
            //        client.server.OutPackets.Enqueue(packet);
            //        bool fakeSend = new Random().Next(1, 3) == 1; 
            //        //client.server.Send(fakeSend);
            //    }
            //}

            Console.ReadLine();
        }
    }
}
