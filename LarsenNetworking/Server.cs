using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Server : NetBase
    { 
		public Server(uint maxPlayers)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), DEFAULT_PORT);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Ip = endPoint.Address.ToString();
            Port = (ushort)endPoint.Port;
            MaxPlayers = maxPlayers;
            Socket.Bind(endPoint);
            IsBound = true;
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
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[6000];
            Data packet;

            while (IsBound)
            {
                try
                {
                    if (Socket.Available > 0)
                    {
                        int dataSize = Socket.ReceiveFrom(buffer, ref sender);

                        packet = Packet.Unpack(buffer);
                        
                        switch ((Request)buffer[0])
                        {
                            case Request.Connection:
                                if (!Players.ContainsKey(sender))
                                    ConnectPlayer(sender);
                                break;

                            case Request.Disconnection:
                                if (Players.ContainsKey(sender))
                                    DisconnectPlayer(sender);
                                break;

                            case Request.Traffic:
                                if (Players.ContainsKey(sender))
                                    ProcessPacket(buffer);
                                break;

                            case Request.RPC:
                                if (Players.ContainsKey(sender))
                                    ProcessRPC();
                                break;

                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
                catch (InvalidCastException e) { Console.WriteLine($"/!\\ Failed to read request /!\\ : {e.Message}"); }
                catch (NotSupportedException e) { Console.WriteLine($"/!\\ Request not supported /!\\ : {e.Message}"); }
            }

            try
            {
                foreach (var player in Players)
                {
                    Socket.SendTo(new byte[1], player.Key);
                }
            }
            catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
        }

        public enum Request
        {
            Connection,
            Disconnection,
            Traffic,
            RPC
        }

        [Request]
        public static void ProcessRPC()
        {
            throw new NotImplementedException();
        }

        [Request]
        public static void ProcessPacket(byte[] packet)
        {
            throw new NotImplementedException();
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
    }
}
