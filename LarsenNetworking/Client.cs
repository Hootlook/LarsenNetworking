using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Data;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public Connection Server { get; set; }
        public IPEndPoint ServerIp { get; private set; }
        public ushort RemoteSlot { get; set; }
        public ConnectionState State { get; set; }

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        public bool Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            ClientIp = ResolveHost("127.0.0.1", DEFAULT_PORT);
            ServerIp = ResolveHost(host, port);
            State = ConnectionState.Disconnected;
            Server = new Connection(ServerIp, this);

            IPEndPoint remoteIp = Server.Ip;

            int clientSalt = Salt.Next(int.MinValue, int.MaxValue);
            int serverSalt = 0;
            int retry = 0;

            while (State != ConnectionState.Connected)
            {
                switch (State)
                {
                    case ConnectionState.Disconnected:
                        byte[] connectionRequest = new ConnectionRequest(clientSalt).GetBytes();
                        Socket.Send(connectionRequest, connectionRequest.Length, ServerIp);

                        if (Socket.Available > 0)
                        {
                            if (Command.TryUnpack(Socket.Receive(ref remoteIp)) is ConnectionRequest challenge)
                            {
                                serverSalt = challenge.ClientSalt;
                                State++;
                            }
                        }
                        break;

                    case ConnectionState.Connecting:
                        byte[] challengeResponse = new ConnectionRequest(clientSalt, clientSalt ^ serverSalt).GetBytes();
                        Socket.Send(challengeResponse, challengeResponse.Length, ServerIp);

                        if (Socket.Available > 0)
                            if (Command.TryUnpack(Socket.Receive(ref remoteIp)) is ConnectionRequest confirmation)
                                if (confirmation.ClientSalt == (clientSalt ^ serverSalt))
                                    State++;
                        break;
                }

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

                Server.Send();
            }
        }

        void Loop()
        {
            IPEndPoint server = ServerIp;

            while (IsBound)
            {
                if (Socket.Available > 0)
                {
                    Server.Receive(Socket.Receive(ref server));
                }

                if (Tick.IsNow())
                {
                    Tick.Reset();

                    Server.Send();
                }
            }
        }

        public void Send(Command command, bool fakeSend) => Server.Send(command, fakeSend);

        public override void Update() => Server.Update();


    }
}
