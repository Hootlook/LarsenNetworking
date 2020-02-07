using System;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class UDPServer : NetEntity, IServer
    {
		public UDPServer(ushort port)
        {
			var address = ResolveHost("localhost", port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Ip = address.Address.ToString();
            Port = (ushort)address.Port;
            Socket.Bind(address);
        }
	}
}
