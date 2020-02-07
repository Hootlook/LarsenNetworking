using LarsenNetworking;
using System; 

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new UDPServer(25250);
           
            Console.WriteLine($"{server.Ip}:{server.Port}");
            Console.ReadLine();
        }
    }
}
