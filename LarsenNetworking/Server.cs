using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LarsenNetworking
{
    public class Server : Networker
    {
        public Dictionary<IPEndPoint, Connection> Clients { get; set; }
        public Dictionary<int, int> Pendings { get; set; }
        
        public uint MaxClients { get; set; } = 24;
        public int ConnectedClients { get; set; }
        public bool[] ClientConnected { get; set; }
        public IPEndPoint[] Addresses { get; set; }
        public Connection[] Connections { get; set; }

        private int FindFreeClientIndex()
        {
            for (int i = 0; i < MaxClients; ++i )
            {
                if ( !ClientConnected[i] )
                    return i;
            }
            return -1;
        }

        private int FindExistingClientIndex(IPEndPoint address)
        {
            for (int i = 0; i < MaxClients; ++i)
            {
                if (ClientConnected[i] && Addresses[i] == address)
                    return i;
            }
            return -1;
        }

        private bool IsClientConnected(int clientIndex) => ClientConnected[clientIndex];

        private IPEndPoint GetClientAddress(int clientIndex) => Addresses[clientIndex];

        public void Host(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            ClientIp = ResolveHost(host, port);
            Socket.Client.Bind(ClientIp);
            Clients = new Dictionary<IPEndPoint, Connection>();
            Pendings = new Dictionary<int, int>();

            ClientConnected = new bool[MaxClients];
            Addresses = new IPEndPoint[MaxClients];

            Task.Run(Receive);
            Task.Run(Sending);
        }

        public override void Receive()
        {
            IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);

            while (IsBound)
            {
                IPEndPoint sender = any;

                byte[] buffer = Socket.Receive(ref sender);
                Console.WriteLine("Server Received");
                if (!Clients.ContainsKey(sender))
                {
                    if (Command.TryUnpack(buffer) is ConnectionRequest request)
                    {
                        if (!Pendings.TryGetValue(request.ClientSalt, out int storedSalt))
                        {
                            int serverSalt = Salt.Next(int.MinValue, int.MaxValue);
                            byte[] challenge = new ConnectionRequest(serverSalt).GetBytes();
                            Socket.Send(challenge, challenge.Length, sender);
                            Pendings.Add(request.ClientSalt, serverSalt);
                        }
                        else
                        {
                            if (request.ChallengeSalt == (request.ClientSalt ^ storedSalt))
                            {
                                byte[] confirmation = new ConnectionRequest(request.ClientSalt).GetBytes();
                                Socket.Send(confirmation, confirmation.Length, sender);
                                Clients.Add(sender, new Connection(sender, this));
                            }

                            Pendings.Remove(request.ClientSalt);
                        }
                    }
                }
                else
                {
                    Clients[sender].Receive(buffer);
                }
            }
        }

        public override void Sending()
        {
            while (IsBound)
            {
                Thread.Sleep(Tick.Rate);

                foreach (var client in Clients.Values.ToArray())
                    client.Send();
            }
        }

        public void Loop()
        {
            IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint sender;
            byte[] buffer;

            while (IsBound)
            {
                if (Socket.Available > 0)
                {
                    sender = any;

                    buffer = Socket.Receive(ref sender);

                    if (!Clients.ContainsKey(sender))
                        Clients.Add(sender, new Connection(sender, this));

                    Clients[sender].Receive(buffer);
                }

                if (Tick.IsNow())
                {
                    Tick.Reset();

                    foreach (var client in Clients.Values)
                        client.Send();
                }
            }
        }

        public override void Update()
        {
            foreach (var client in Clients.Values.ToArray())
                client.Update();
        }
    }
}
