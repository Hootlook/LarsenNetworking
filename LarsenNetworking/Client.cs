using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        NetPlayer server;

        public void Send(IMessage message) => server.Send(message);

        public bool Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            PeerIp = ResolveHost(host, port);

            server = new NetPlayer(PeerIp, Socket);
            IPEndPoint serverIp = server.Ip;

            int retry = 0;
            bool success = false;
            while (!success)
            {
                server.Send();

                if (Socket.Available > 0)
                    success = Socket.Receive(ref serverIp).Length > 0;

                //if (Socket.Available > 0)
                //    server.Receive(Socket.Receive(ref serverIp));

                //for (int i = 0; i < server.ReceivedCommands.Count; i++)
                //    if (server.ReceivedCommands.Dequeue().Message is ConnectionMessage)
                //        success = true;

                Thread.Sleep(1000);

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
            byte[] buffer;

            while (true)
            {
                try
                {
                    if (Socket.Available > 0)
                    {
                        buffer = Socket.Receive(ref serverIp);

                        server.Receive(buffer);

                        for (int i = 0; i < server.ReceivedCommands.Count; i++)
                            server.ReceivedCommands.Dequeue().Message.Execute();
                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    server.Send(new Random().Next(1, 3) == 1);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }
    }
    public class ConnectionMessage : IMessage
    {
        public void Execute()
        {

        }
    }
}
