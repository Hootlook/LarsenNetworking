using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public NetPlayer server;

        public void Send(IMessage message) => server.Send(message);

        public bool Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            PeerIp = ResolveHost(host, port);

            server = new NetPlayer(PeerIp, Socket);
            IPEndPoint remoteIp = server.Ip;

            int retry = 0;
            bool success = false;
            while (!success)
            {
                server.Send();

                if (Socket.Available > 0)
                    success = Socket.Receive(ref remoteIp).Length > 0;

                Thread.Sleep(1000);

                if (retry++ > 5) return false;
            }

            try
            {
                Task.Run(() =>
                {
                    IPEndPoint serverIp = server.Ip;
                    byte[] buffer;

                    while (true)
                    {
                        if (Socket.Available > 0)
                        {
                            buffer = Socket.Receive(ref serverIp);

                            server.Receive(buffer);

                            for (int i = 0; i < server.ReceivedCommands.Count; i++)
                                server.ReceivedCommands.Dequeue().Message.Execute();
                        }
                    }
                });

                Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000 / TickRate);
                        server.Send();
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"<<< CLIENT CRASHED >>> : {e.Message}");
                Socket.Dispose();
                throw;
            }

            return true;
        }
    }

    public class ConnectionMessage : IMessage
    {
        public void Execute()
        {

        }
    }
}
