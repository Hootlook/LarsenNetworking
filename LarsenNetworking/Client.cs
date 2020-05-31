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

        private void Routine()
        {
            PacketHandler packetHandler = new PacketHandler(Socket);

            Command.Register(new IMessage[] { new PrintMessage(0, 0, 0) });

            while (IsBound)
            {
                Packet packet = Packet.Empty;

                packet.WriteCommand(new Command(new PrintMessage(
                    packetHandler.Ack,
                    packetHandler.Sequence,
                    packetHandler.AckBits)));

                packetHandler.OutGoingPackets.Enqueue(packet);

                Thread.Sleep(RunSpeed);

                try
                {
                    packetHandler.Receive(PeerIp);

                    Console.WriteLine(
                        $"//////////////////// LOCAL //////////////////////\n" + 
                        $"Sequence : {packetHandler.Sequence}\n" +
                        $"Ack : {packetHandler.Ack}\n" +
                        $"AckBits : {Convert.ToString(packetHandler.AckBits, 2).PadLeft(PacketHandler.BUFFER_SIZE, '0')}\n" +
                        $"CurrentBit : {packetHandler.Ack % PacketHandler.BUFFER_SIZE}\n" +
                        $"PacketAvailable : {packetHandler.Socket.Available}\n" +
                        $"RunningSpeed : {RunSpeed}ms\n" +
                        $"/////////////////////////////////////////////////\n"
                        );

                    if (packetHandler.InComingPackets.Count != 0)
                        packetHandler.InComingPackets.Dequeue().Messages[0].Message.Execute();

                    Console.SetCursorPosition(0, 2);

                }
                catch (Exception e) { Console.WriteLine($"/!\\ Receiving error /!\\ : {e.Message}"); }

                try
                {
                    bool random = new Random().Next(6) == 0;
                    packetHandler.Send(PeerIp, random);
                }
                catch (Exception e) { Console.WriteLine($"/!\\ Broadcast error /!\\ : {e.Message}"); }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.Add:
                            RunSpeed += 10;
                            break;
                        case ConsoleKey.Subtract:
                            RunSpeed -= 10;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        protected override void Initialisation()
        {
            throw new NotImplementedException();
        }
    }
}
