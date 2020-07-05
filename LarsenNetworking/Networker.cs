using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace LarsenNetworking
{
	public abstract class Networker
	{
		public const ushort DEFAULT_PORT = 26950;
		public const ushort BUILD_VERSION = 1;
		public const string CONNECT_MESSAGE = "Hello i'd like to play pls";
		private const int SIO_UDP_CONNRESET = -1744830452;

		public enum State
		{
			Disconnected,
			Initialising,
			Connected,
		}

        public Dispatcher MainDispatcher { get; set; }
        public bool IsBound { get { return Socket.Client.IsBound; } }
		public State CurrentState { get; set; } = State.Disconnected;
		public IPEndPoint PeerIp { get; set; }
		public IPEndPoint Ip { get; set; }
		public int RunSpeed { get; set; } = 100;
		public bool IsServer { get { return this is Server; } }
		public Time Time { get; set; }
		public uint MaxPlayers { get; set; }
		public UdpClient Socket { get; set; }
		public Dictionary<EndPoint, NetPlayer> Players { get; set; } = new Dictionary<EndPoint, NetPlayer>();

		public Networker()
		{
			Ip = ResolveHost("127.0.0.1", DEFAULT_PORT);
			Socket = new UdpClient();
			Time = new Time();
			//MainDispatcher = Dispatcher.CurrentDispatcher; 
				
			Socket.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
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
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						if (ip.ToString() == host)
							return new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
					}
				}

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
	}
}
