using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Server : Networker
    { 
		public Server(uint maxPlayers)
        {
            Ip = ResolveHost("127.0.0.1", DEFAULT_PORT + 1);
            PeerIp = ResolveHost("127.0.0.1", DEFAULT_PORT);
            Socket.Client.Bind(Ip);
            MaxPlayers = maxPlayers;
        }

        public void Run()
        {
            try
            {
                Task.Run(Routine);
            }
            catch (Exception e)
            {
                Console.WriteLine($"<<< SERVER CRASHED >>> : {e.Message}");
                Socket.Dispose();
                throw;
            }
        }

        private void Routine()
        {
            IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint sender;
            NetPlayer player;
            Packet packet;
            byte[] buffer;

            while (IsBound)
            {
                sender = any;

                try
                {
                    if (Socket.Available > 0)
                    {
                        buffer = Socket.Receive(ref sender);

                        if (!Players.ContainsKey(sender))
                            Players.Add(sender, new NetPlayer(sender, Socket));

                        player = Players[sender];

                        player.Receive(buffer);

                        if (player.InComingPackets.Count != 0)
                        {
                            packet = player.InComingPackets.Dequeue();
                            for (int i = 0; i < packet.Messages.Count; i++)
                            {
                                if (packet.Messages[i].Message is Client.ConnectionMessage)
                                {
                                    Packet answere = Packet.Empty;
                                    answere.WriteCommand(new Command(new Client.ConnectionMessage()));
                                    player.OutGoingPackets.Enqueue(answere);
                                    player.Send();
                                }
                                else
                                {
                                    packet.Messages[i].Message.Execute();
                                }
                            }
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    foreach (NetPlayer netPlayer in Players.Values)
                    {
                        netPlayer.Send();
                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }        
    }
}
