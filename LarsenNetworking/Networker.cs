using System;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LarsenNetworking
{
    public abstract class Networker
	{
		public const ushort DEFAULT_PORT = 26950;
		public const ushort BUILD_VERSION = 1;
		private const int SIO_UDP_CONNRESET = -1744830452;

		public bool IsBound { get { return Socket.Client.IsBound; } }
		public IPEndPoint ClientIp { get; set; }
		public UdpClient Socket { get; private set; }
		public Tick Tick { get; private set; }
        public Stopwatch Time { get; private set; }

        public Networker()
		{
			Socket = new UdpClient();
			Time = new Stopwatch();
			Tick = new Tick(Time);

			Time.Start();

            Socket.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
        }

		~Networker() => Socket?.Dispose();

        public abstract void Receive();
        public abstract void Sending();
        public abstract void Update();

		void StartWorking()
        {
            #region ByTask
            //Task.Run(() =>
            //{
            //    IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
            //    IPEndPoint sender;
            //    Connection player;
            //    byte[] buffer;

            //    while (IsBound)
            //    {
            //        sender = ServerIp ?? any;

            //        buffer = Socket.Receive(ref sender);

            //        if (IsServer && !Clients.ContainsKey(sender))
            //            Clients.Add(sender, new Connection(sender, this));

            //        player = Server ?? Clients[sender];

            //        player.Receive(buffer);
            //    }
            //});

            //Task.Run(async () =>
            //{
            //    while (IsBound)
            //    {
            //        await Task.Delay(1000 / Tick.Rate);

            //        if (IsServer)
            //            foreach (var player in Clients)
            //                player.Value.Send();

            //        Server?.Send(fakeSend: new Random().Next(1, 3) == 1);
            //    }
            //});
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
            //        Clients.Add(sender, new Connection(sender, this));

            //    player = Server ?? Clients[sender];

            //    player.Receive(buffer);
            //};
            //r.Start();

            //System.Timers.Timer s = new System.Timers.Timer(1000 / Tick.Rate);
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
            //        Clients.Add(sender, new Connection(sender, this));

            //    player = Server ?? Clients[sender];

            //    player.Receive(buffer);
            //}, null, 0, Timeout.Infinite);

            //System.Threading.Timer g = new System.Threading.Timer(s =>
            //{
            //    if (IsServer)
            //        foreach (var client in Clients)
            //            client.Value.Send();

            //    Server?.Send(fakeSend: new Random().Next(1, 3) == 1);
            //}, null, 0, 1000 / Tick.Rate);
            #endregion
        }

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

        #region ThreadManagerExample
        private static readonly List<Command> pendingForSend = new List<Command>();
        private static readonly List<Command> pendingForUpdate = new List<Command>();
        private static bool hasActionsForMainThread = false;

        public static void ExecuteOnMainThread(Command command)
        {
            if (command == null) return;

            lock (pendingForSend)
            {
                pendingForSend.Add(command);
                hasActionsForMainThread = true;
            }
        }

        public static void UpdateMain()
        {
            if (hasActionsForMainThread)
            {
                pendingForUpdate.Clear();
                lock (pendingForSend)
                {
                    pendingForUpdate.AddRange(pendingForSend);
                    pendingForSend.Clear();
                    hasActionsForMainThread = false;
                }

                for (int i = 0; i < pendingForUpdate.Count; i++)
                {
                    pendingForUpdate[i].Execute();
                }
            }
        }
        #endregion
    }
}
