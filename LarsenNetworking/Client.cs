using System;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class Client : NetEntity
    {
        public void Connect(string host = "localhost", ushort port = DEFAULT_PORT)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), DEFAULT_PORT);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Ip = endPoint.Address.ToString();
            Port = (ushort)endPoint.Port;
            Socket.Bind(new IPEndPoint(endPoint.Address, endPoint.Port + 1));

            while (true)
            {
                Socket.SendTo(new byte[1], endPoint);
            }
        }
    }
}
