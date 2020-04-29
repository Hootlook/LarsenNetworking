using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Server : Networker
    { 
		public Server(uint maxPlayers)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), DEFAULT_PORT);
            Ip = endPoint.Address.ToString();
            Port = (ushort)endPoint.Port;
            MaxPlayers = maxPlayers;
            Socket.Bind(endPoint);
            IsBound = true;
        }

        public void Run()
        {
            try
            {
                Task.Run(Routine);
            }
            catch (Exception e)
            {
                Console.WriteLine($"<<< SERVER CRASHED >>> : {e.Message}");
                Socket.Dispose();
                throw;
            }
        }

        private void Routine()
        {
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            DateTime lastLoop = DateTime.Now; 
            byte[] buffer = new byte[6000];
            //NetPlayer currentPlayer;
            Packet packet;

            while (IsBound)
            {
                try
                {
                    if (Socket.Available > 0)
                    {
                        int dataSize = Socket.ReceiveFrom(buffer, ref sender);

                        //packet = Packet.Unpack(buffer);
                        
                        //Rpc rpc = Rpc.list[packet.rpc];

                        //if (!Players.ContainsKey(sender))
                        //    if (rpc.Label is BaseRpc.Connect)
                        //        continue;

                        //rpc.Action.Invoke(packet);

                        //Rpc.toSend.Enqueue(rpc);
                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    Time.TimeStep++;

                    Rpc nextRpc = Rpc.toSend.Dequeue();

                    foreach (var player in Players)
                    {
                        Socket.SendTo(new byte[1], player.Key);
                    }
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }        

        private void DisconnectPlayer(EndPoint sender)
        {
            throw new NotImplementedException();
        }

        private void ConnectPlayer(EndPoint sender)
        {
            var player = new NetPlayer
            {
                Ip = ((IPEndPoint)sender).Address,
                Name = $"Player {Players.Count + 1}"
            };

            Players.Add(sender, player);
            Console.WriteLine($"{player.Name} ({player.Ip}) Joined");
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }

        
    }
}
