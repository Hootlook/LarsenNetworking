using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public NetPlayer server;

        public bool Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            PeerIp = ResolveHost(host, port);

            server = new NetPlayer(PeerIp, Socket);
            IPEndPoint serverIp = server.Ip;

            Packet packet = Packet.Empty;
            packet.WriteCommand(new Command(new ConnectionMessage()));

            byte[] buffer = new byte[0];

            int retry = 0;
            while (true)
            {
                if (Socket.Available > 0)
                    buffer = Socket.Receive(ref serverIp);

                server.Receive(buffer);

                if (server.InComingPackets.Count > 0)
                    if (server.InComingPackets.Dequeue().Messages[0].Message is ConnectionMessage)
                        break;

                Thread.Sleep(1000);

                server.OutGoingPackets.Enqueue(packet);
                server.Send();

                if (retry++ > 5) return false;
            }

            try
            {
                Task.Run(Routine);
            }
            catch (Exception e)
            {
                Console.WriteLine($"<<< CLIENT CRASHED >>> : {e.Message}");
                Socket.Dispose();
                throw;
            }

            return true;
        }

        private void Routine()
        {
            IPEndPoint serverIp = server.Ip;
            Packet packet;
            byte[] buffer;

            while (IsBound)
            {
                try
                {
                    if (Socket.Available > 0)
                    {
                        buffer = Socket.Receive(ref serverIp);

                        server.Receive(buffer);

                        if (server.InComingPackets.Count != 0)
                        {
                            packet = server.InComingPackets.Dequeue();
                            for (int i = 0; i < packet.Messages.Count; i++)
                                packet.Messages[i].Message.Execute();
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    server.Send();
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }

        public class ConnectionMessage : IMessage
        {
            public void Execute()
            {
                
            }
        }
    }
}
