using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public IPEndPoint ServerIp { get; set; }
        public void Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            ServerIp = new IPEndPoint(IPAddress.Parse(host), port);
            PacketHandler packetHandler = new PacketHandler();
            EndPoint sender = ServerIp;
            byte[] buffer = new byte[1500];

            var packet = new Packet();
            //packet.WriteMessage(1, new[] { CONNECT_MESSAGE });

            while (true)
            {
                Socket.SendTo(packet.Pack(), ServerIp);

                Thread.Sleep(1000);

                int dataSize = Socket.ReceiveFrom(buffer, ref sender);

                if (dataSize > 0)
                {

                }

            }
        }

        private void Routine()
        {
            while (true)
            {
                EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                DateTime lastLoop = DateTime.Now;
                byte[] buffer = new byte[6000];
                NetPlayer currentPlayer;
                Packet packet;

                try
                {
                    if (Socket.Available > 0)
                    {
                        int dataSize = Socket.ReceiveFrom(buffer, ref sender);

                        //packet = Packet.Unpack(buffer);
                        Console.WriteLine(sender);

                        if (Players.ContainsKey(sender))
                        {
                            currentPlayer = Players[sender];

                        }
                        else
                        {
                            //Packet.Unpack(data.data)
                        }

                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    Time.TimeStep++;

                    //byte[] packetToSend = Packet.Pack(new Packet { ack = true, rpc = 50, frame = 100 });

                    //Socket.SendTo(packetToSend, ServerIp);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }
    }
}
