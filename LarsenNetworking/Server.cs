using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Server : NetEntity
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

        public void Start()
        {
            try
            {
                Task.Run(NetworkLoop);
            }
            catch (Exception e)
            {
                Console.WriteLine($"<<< SERVER CRASHED >>> : {e.Message}");
                throw e;
            }
        }

        private void NetworkLoop()
        {
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] packet = new byte[5000];

            while (IsBound)
            {
                try
                {
                    int dataSize = Socket.ReceiveFrom(packet, ref sender);

                    switch ((Request)packet[0])
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
                                ProcessPacket(packet);
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                }
                catch (InvalidCastException e) { Console.WriteLine($"/!\\ Failed to read packet /!\\ : {e.Message}"); }
                catch (NotSupportedException e) { Console.WriteLine($"/!\\ Request not supported /!\\ : {e.Message}"); }
            }
        }

        public enum Request
        {
            Connection,
            Disconnection,
            Traffic,
        }

        private void ProcessPacket(byte[] packet)
        {
            throw new NotImplementedException();
        }

        private void DisconnectPlayer(EndPoint sender)
        {
            throw new NotImplementedException();
        }

        private void ConnectPlayer(EndPoint sender)
        {
            throw new NotImplementedException();
        }
    }
}
