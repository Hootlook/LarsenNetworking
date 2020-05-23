using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    public class Client : Networker
    {
        public void Connect(string host = "127.0.0.1", ushort port = DEFAULT_PORT + 1)
        {
            PeerIp = ResolveHost(host, port);
            Socket.Client.Bind(Ip);

            try
            {
                Task.Run(Routine);
            }
            catch (Exception e)
            {
                Console.WriteLine($"<<< CLIENT CRASHED >>> : {e.Message}");
                Socket.Dispose();
                throw;
            }
        }

        void Routine()
        {
            PacketHandler packetHandler = new PacketHandler(Socket);

            Command.Register(new IMessage[] { new PrintMessage("none", "none", "none") });

            while (IsBound)
            {
                Packet packet = Packet.Empty;

                packet.WriteCommand(new Command(new PrintMessage(
                    packetHandler.Ack.ToString(), 
                    packetHandler.Sequence.ToString(), 
                    packetHandler.AckBits.ToString())));

                packetHandler.OutGoingPackets.Enqueue(packet);

                Thread.Sleep(500);

                try
                {
                    packetHandler.Receive(PeerIp);

                    Console.WriteLine("//////////////////// LOCAL //////////////////////");
                    Console.WriteLine(
                        $"Sequence : {packetHandler.Sequence}\n" +
                        $"Ack : {packetHandler.Ack}\n" +
                        $"AckBits : {packetHandler.AckBits}"
                        );
                    Console.WriteLine("////////////////////////////////////////////////");

                    if (packetHandler.InComingPackets.Count != 0)
                        packetHandler.InComingPackets.Dequeue().Messages[0].Message.Execute();

                    Console.SetCursorPosition(0, 1);

                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    packetHandler.Send(PeerIp);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }
            }
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }
    }
}
