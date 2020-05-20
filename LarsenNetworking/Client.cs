using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public void Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            PeerIp = ResolveHost(host, port);
            Socket.Client.Bind(Ip);

            PacketHandler packetHandler = new PacketHandler(Socket);

            Command.Register(new IMessage[] { new PrintMessage("") });            
            int id = 0;

            while (IsBound)
            {
                Packet packet = Packet.Empty;
                packet.WriteCommand(new Command(new PrintMessage(id.ToString())));
                packetHandler.OutGoingPackets.Enqueue(packet);
                id++;

                try
                {
                    packetHandler.Receive(PeerIp);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    packetHandler.Send(PeerIp);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }

                Console.Read();
            }
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }
    }
}
