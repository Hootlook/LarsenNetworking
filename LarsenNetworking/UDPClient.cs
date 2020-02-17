using System;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class UDPClient : NetEntity
    {
        public void Connect(string host = "localhost", ushort port = DEFAULT_PORT)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), DEFAULT_PORT);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Ip = endPoint.Address.ToString();
            Port = (ushort)endPoint.Port;
            Socket.Bind(endPoint);

            byte[] packet = new byte[1];

        }
    }
}
