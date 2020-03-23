using System;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public IPEndPoint ServerIp { get; set; }
        public void Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            ServerIp = new IPEndPoint(IPAddress.Parse(host), port);

            while (true)
            {
                var packet = Packet.Pack(new Packet {  ack = true, rpc = 50, frame = 100 });
                
                Socket.SendTo(packet, ServerIp);
            }
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }
    }
}
