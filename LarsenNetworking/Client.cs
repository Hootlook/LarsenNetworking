using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public Connection Server { get; set; }
        public IPEndPoint ServerIp { get; private set; }

        public bool Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            ClientIp = ResolveHost("127.0.0.1", DEFAULT_PORT);
            ServerIp = ResolveHost(host, port);
            Server = new Connection(ServerIp, this);

            IPEndPoint remoteIp = Server.Ip;

            int retry = 0;
            bool success = false;
            while (!success)
            {
                Server.Send();

                if (Socket.Available > 0)
                    success = Socket.Receive(ref remoteIp).Length > 0;

                Thread.Sleep(1000);

                if (retry++ > 5) return false;
            }

            Task.Run(Receive);
            Task.Run(Sending);

            return true;
        }

        public override void Receive()
        {
            IPEndPoint server = ServerIp;

            while (IsBound)
            {
                Server.Receive(Socket.Receive(ref server));
            }
        }

        public override void Sending()
        {
            while (IsBound)
            {
                Thread.Sleep(Tick.Rate);

                Server.Send(fakeSend: new Random().Next(1, 3) == 1);
            }
        }

        public void Send(Command command, bool fakeSend) => Server.Send(command, fakeSend);

        public override void Update()
        {
            for (int i = 0; i < Server.ReceivedCommands.Count; i++)
                Server.ReceivedCommands[i].Execute();
        }
    }
}
