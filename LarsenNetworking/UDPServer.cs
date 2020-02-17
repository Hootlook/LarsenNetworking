using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class UDPServer : NetEntity, IServer
    { 
		public UDPServer(uint maxPlayers)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), DEFAULT_PORT);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Ip = endPoint.Address.ToString();
            Port = (ushort)endPoint.Port;
            MaxPlayers = maxPlayers;
            Socket.Bind(endPoint);
        }

        public void Start()
        {
            IsBound = true;

            try
            {
                Task.Run(NetworkLoop);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void NetworkLoop()
        {
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] packet = new byte[1];
            int dataSize = 0;

            while (IsBound)
            {
                try
                {
                    dataSize = Socket.ReceiveFrom(packet, ref sender);
                    Console.WriteLine(sender.ToString() + " with " + packet.Length + " of " + dataSize);
                }
                catch (Exception e)
                {
                    NetPlayer player;
                    if(Players.TryGetValue(sender, out player))
                    {
                        DisconnectPlayer(player);
                    }
                    Console.WriteLine("Reading exeption : " + e.Message);
                }

                if (dataSize == 0) continue;

                if (!Players.ContainsKey(sender))
                {
                    ConnectPlayer(packet, sender);
                    continue;
                }
                else
                {
                    ReadPacket(packet);
                }
            }
        }

        private void ReadPacket(byte[] packet)
        {
            throw new NotImplementedException();
        }

        private void DisconnectPlayer(NetPlayer player)
        {
            throw new NotImplementedException();
        }

        private void ConnectPlayer(byte[] packet, EndPoint sender)
        {
            throw new NotImplementedException();
        }
    }
}
