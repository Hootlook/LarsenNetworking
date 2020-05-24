using System;
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
            PacketHandler packetHandler = new PacketHandler(Socket);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            Command.Register(new IMessage[] { new PrintMessage("", "", "") });

            while (IsBound)
            {
                Packet packet = Packet.Empty;

                packet.WriteCommand(new Command(new PrintMessage(
                    packetHandler.Ack.ToString(),
                    packetHandler.Sequence.ToString(),
                    packetHandler.AckBits.ToString())));

                packetHandler.OutGoingPackets.Enqueue(packet);

                Thread.Sleep(500);

                try
                {
                    packetHandler.Receive(sender);

                    Console.WriteLine(
                        $"//////////////////// LOCAL //////////////////////\n" +
                        $"Sequence : {packetHandler.Sequence}\n" +
                        $"Ack : {packetHandler.Ack}\n" +
                        $"AckBits : {packetHandler.AckBits}\n" +
                        $"/////////////////////////////////////////////////\n"
                        );

                    if (packetHandler.InComingPackets.Count != 0)
                        packetHandler.InComingPackets.Dequeue().Messages[0].Message.Execute();

                    Console.SetCursorPosition(0, 2);

                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    packetHandler.Send(PeerIp);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }        

        private void DisconnectPlayer(EndPoint sender)
        {
            throw new NotImplementedException();
        }

        private void ConnectPlayer(EndPoint sender)
        {
            var player = new NetPlayer
            {
                Ip = ((IPEndPoint)sender).Address,
                Name = $"Player {Players.Count + 1}"
            };

            Players.Add(sender, player);
            Console.WriteLine($"{player.Name} ({player.Ip}) Joined");
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }
    }
}
