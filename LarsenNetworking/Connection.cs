using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static LarsenNetworking.Command;

namespace LarsenNetworking
{
    public class Connection
    {
        public IPEndPoint Ip { get; set; }
        public UdpClient Socket { get; set; }

        public Connection(IPEndPoint ip, UdpClient socket)
        {
            Socket = socket;
            Ip = ip;
        }

        #region PacketHandling
        private object CommandsLock { get; set; } = new object();
        public List<Command> SendingCommands { get; set; } = new List<Command>();
        public Queue<Command> ReceivedCommands { get; set; } = new Queue<Command>();
        public int MtuLimit { get; set; } = MTU_LIMIT;
        public ushort Sequence { get; set; } 
        public ushort Ack { get; set; }
        public ushort OrderId { get; set; }

        public const int BUFFER_SIZE = 32;
        public const int MTU_LIMIT = 1408;
        public struct PacketData
        {
            public bool acked;
            public int time;
        }
        private uint[] sequenceBuffer = new uint[BUFFER_SIZE];
        private PacketData[] packetDatas = new PacketData[BUFFER_SIZE];
        public ref PacketData InsertPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            sequenceBuffer[index] = sequence;
            return ref packetDatas[index];
        }

        public PacketData? GetPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            if (sequenceBuffer[index] == sequence)
                return packetDatas[index];
            else
                return null;
        }

        public void Send(Command command = null, bool fakeSend = false)
        {
            lock (CommandsLock)
            {
                Packet outGoingPacket = Packet.Empty;

                outGoingPacket.Sequence = Sequence++;
                outGoingPacket.Ack = Ack;
                outGoingPacket.AckBits = GenerateAckBits();

                if (command != null)
                {
                    if (command.Method != SendingMethod.Reliable)
                        command.OrderId = OrderId++;

                    if (command.Method == SendingMethod.Unreliable)
                        outGoingPacket.WriteCommand(command);

                    if (command.Method != SendingMethod.Unreliable)
                        SendingCommands.Add(command);
                }

                for (int i = SendingCommands.Count - 1; i >= 0; i--)
                {
                    if ((DateTime.Now - SendingCommands[i].SendTime).TotalMilliseconds < 100) continue;
                    if (SendingCommands[i].Size + outGoingPacket.Data.Count > MtuLimit) continue;

                    SendingCommands[i].PacketId = outGoingPacket.Sequence;
                    SendingCommands[i].SendTime = DateTime.Now;
                    outGoingPacket.WriteCommand(SendingCommands[i]);
                }

                byte[] packet = outGoingPacket.Pack();

                if(!fakeSend)
                    Socket.Send(packet, packet.Length, Ip);
            }
        }

        public void Receive(byte[] buffer)
        {
            lock (CommandsLock)
            {
                Packet receivedPacket = Packet.Unpack(buffer);
                if (receivedPacket == null) return;

                if (receivedPacket.IsNewerThan(Ack))
                    Ack = receivedPacket.Sequence;

                else if (GetPacketData(Ack).Value.acked)
                    return;

                InsertPacketData(receivedPacket.Sequence).acked = true;

                for (int bit = 0; bit < packetDatas.Length; bit++)
                    if ((receivedPacket.AckBits & (1 << bit)) != 0)
                        SendingCommands.RemoveAll(m => m.PacketId == receivedPacket.Ack - bit);

                for (int i = 0; i < receivedPacket.Commands.Count; i++)
                    ReceivedCommands.Enqueue(receivedPacket.Commands[i]);
            }
        }

        public uint GenerateAckBits()
        {
            uint bits = 0;
            uint mask = 1;

            for (uint i = 0; i < packetDatas.Length; i++)
            {
                uint index = (Ack - i) % BUFFER_SIZE;

                uint sequence = sequenceBuffer[index];
                bool acked = packetDatas[index].acked;

                if (acked && (sequence >= Ack - BUFFER_SIZE && sequence <= Ack))
                    bits |= mask;
                mask <<= 1;
            }

            return bits;
        }

        public void Update()
        {
            lock (CommandsLock)
            {
                var ReliableOrdered = ReceivedCommands.Where(c => c.Method == SendingMethod.ReliableOrdered).OrderBy(c => c.OrderId);
                var Reliable = ReceivedCommands.Where(c => c.Method == SendingMethod.Reliable);
                var Unreliable = ReceivedCommands.Where(c => c.Method == SendingMethod.Unreliable).OrderByDescending(c => c.OrderId).LastOrDefault();

                ReliableOrdered.ToList().ForEach(c => c.Execute());
                Reliable.ToList().ForEach(c => c.Execute());
                Unreliable?.Execute();

                ReceivedCommands.Clear();
            }
        }
        #endregion
    }
}