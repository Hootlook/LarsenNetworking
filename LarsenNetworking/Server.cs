using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Server : Networker
    {
        public uint MaxPlayers { get; set; } = 24;

        public Dictionary<IPEndPoint, Connection> Clients { get; set; }

        public void Host(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            ClientIp = ResolveHost(host, port);
            Socket.Client.Bind(ClientIp);
            Clients = new Dictionary<IPEndPoint, Connection>();

            Task.Run(Receive);
            Task.Run(Sending);
        }

        public override void Receive()
        {
            IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint sender;
            byte[] buffer;

            while (IsBound)
            {
                sender = any;

                buffer = Socket.Receive(ref sender);

                if (!Clients.ContainsKey(sender))
                    Clients.Add(sender, new Connection(sender, this));

                Clients[sender].Receive(buffer);
            }
        }

        public override void Sending()
        {
            while (IsBound)
            {
                Thread.Sleep(Tick.Rate);

                foreach (var client in Clients.Values)
                    client.Send();
            }
        }

        public override void Update()
        {
            for (int j = 0; j < Clients.Count; j++)
                for (int i = 0; i < Clients.Values.ToArray()[j].ReceivedCommands.Count; i++)
                    Clients.Values.ToArray()[j].Update();
        }
    }
}
