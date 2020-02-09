using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class UDPServer : NetEntity, IServer
    {
		public UDPServer(uint maxPlayers)
        {
			var endPoint = ResolveHost("localhost", DEFAULT_PORT);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Ip = endPoint.Address.ToString();
            Port = (ushort)endPoint.Port;
            MaxPlayers = maxPlayers;
            Socket.Bind(endPoint);
        }

        public void Start()
        {
            try
            {
                Task.Run(NetworkLoop);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Connect()
        {

        }

        private void NetworkLoop()
        {
            throw new NotImplementedException();
        }
    }
}
