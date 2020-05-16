using System.Net;

namespace LarsenNetworking
{
    public class NetPlayer
    {
        public string Name { get; set; }
        public int Ping { get; set; }
        public IPAddress Ip { get; set; }
        public PacketHandler PacketHandler { get; set; }

        enum State
        {
            Connected,
            Disconnected,
            Retrying,
        }
    }
}