﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

namespace LarsenNetworking
{
	public class Networker
	{
		public const ushort DEFAULT_PORT = 26950;
		public const ushort BUILD_VERSION = 1;
		private const int SIO_UDP_CONNRESET = -1744830452;

		public bool IsBound { get { return Socket.Client.IsBound; } }
		public bool IsServer { get { return Server == null; } }
		public uint MaxPlayers { get; set; } = 24;
        public int RunSpeed { get; set; } = 100;
        public Dictionary<IPEndPoint, Connection> Clients { get; private set; }
        public Connection Server { get; private set; }
		public IPEndPoint ServerIp { get; private set; }
		public IPEndPoint ClientIp { get; private set; }
		public UdpClient Socket { get; private set; }
		public Tick Tick { get; private set; }
		public Tick Update { get; private set; }
        public Stopwatch Time { get; private set; }
		public int TickRate { get; set; } = 30;

        public Networker()
		{
			Socket = new UdpClient();
			Time = new Stopwatch();
			Tick = new Tick(Time);
			Update = new Tick(Time);

			Time.Start();

            Socket.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
        }

		~Networker() => Socket?.Dispose();

		public void Host(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1) 
		{
			ClientIp = ResolveHost(host, port);
			Socket.Client.Bind(ClientIp);
			Clients = new Dictionary<IPEndPoint, Connection>();

			StartWorking(); 
		}

		public bool Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
		{
			ClientIp = ResolveHost("127.0.0.1", DEFAULT_PORT);
			ServerIp = ResolveHost(host, port);
			Server = new Connection(ServerIp, Socket);

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

			StartWorking();

			return true;
		}

		void StartWorking()
        {
            #region ByTask
            Task.Run(() =>
            {
                IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
                IPEndPoint sender;
                Connection player;
                byte[] buffer;

                while (IsBound)
                {
                    sender = ServerIp ?? any;

                    buffer = Socket.Receive(ref sender);

                    if (IsServer && !Clients.ContainsKey(sender))
                        Clients.Add(sender, new Connection(sender, Socket));

                    player = Server ?? Clients[sender];

                    player.Receive(buffer);
                }
            });

            Task.Run(() =>
            {
                while (IsBound)
                {
                    Thread.Sleep(1000 / TickRate);

                    if (IsServer)
                        foreach (var player in Clients)
                            player.Value.Send();

                    Server?.Send(fakeSend: new Random().Next(1, 3) == 1);
                }
            });
            #endregion

            #region ByTimer
            //IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
            //IPEndPoint sender;
            //Connection player;
            //byte[] buffer;

            //System.Timers.Timer r = new System.Timers.Timer();
            //r.Elapsed += (o, e) =>
            //{
            //    sender = ServerIp ?? any;

            //    buffer = Socket.Receive(ref sender);

            //    if (IsServer && !Clients.ContainsKey(sender))
            //        Clients.Add(sender, new Connection(sender, Socket));

            //    player = Server ?? Clients[sender];

            //    player.Receive(buffer);
            //};
            //r.Start();

            //System.Timers.Timer s = new System.Timers.Timer(1000 / TickRate);
            //s.Elapsed += (f, e) =>
            //{
            //    if (IsServer)
            //        foreach (var client in Clients)
            //            client.Value.Send();

            //    Server?.Send(fakeSend: new Random().Next(1, 3) == 1);
            //};
            //s.Start();
            #endregion

            #region ByTimerTask
            //IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
            //IPEndPoint sender;
            //Connection player;
            //byte[] buffer;

            //System.Threading.Timer t = new System.Threading.Timer(s =>
            //{
            //    sender = ServerIp ?? any;

            //    buffer = Socket.Receive(ref sender);

            //    if (IsServer && !Clients.ContainsKey(sender))
            //        Clients.Add(sender, new Connection(sender, Socket));

            //    player = Server ?? Clients[sender];

            //    player.Receive(buffer);
            //}, null, 0, Timeout.Infinite);

            //System.Threading.Timer g = new System.Threading.Timer(s =>
            //{
            //    if (IsServer)
            //        foreach (var client in Clients)
            //            client.Value.Send();

            //    Server?.Send(fakeSend: new Random().Next(1, 3) == 1);
            //}, null, 0, 1000 / TickRate);
            #endregion
        }

        public void Send(Command command, bool fakeSend) => Server?.Send(command, fakeSend);

		public static IPEndPoint ResolveHost(string host, ushort port)
		{
			if (host == "0.0.0.0" || host == "127.0.0.1" || host == "::0")
				return new IPEndPoint(IPAddress.Parse(host), port);
			else if (host == "localhost")
				return new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

			if (!IPAddress.TryParse(host, out IPAddress ipAddress))
			{
				IPHostEntry hostCheck = Dns.GetHostEntry(Dns.GetHostName());

				foreach (IPAddress ip in hostCheck.AddressList)
					if (ip.AddressFamily == AddressFamily.InterNetwork)
						if (ip.ToString() == host)
							return new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

				try
				{
					IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
					ipAddress = ipHostInfo.AddressList[0];
				}
				catch
				{
					throw new ArgumentException("Unable to resolve host");
				}
			}

			return new IPEndPoint(ipAddress, port);
		}

		public class ConnectionMessage : Command
		{
			public override void Execute()
			{
				throw new NotImplementedException();
			}
		}
	}
}
