using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class NetPlayer
    {
        public IPEndPoint Ip { get; set; }
        public UdpClient Socket { get; set; }

        public NetPlayer(IPEndPoint ip, UdpClient socket)
        {
            Socket = socket;
            Ip = ip;
        }

        #region PacketHandling
        public List<Command> SendingCommands { get; set; } = new List<Command>();
        public Queue<Command> ReceivedCommands { get; set; } = new Queue<Command>();
        public int MtuLimit { get; set; } = MTU_LIMIT;
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }

        public const int BUFFER_SIZE = 32;
        public const int MTU_LIMIT = 1408;
        public struct PacketData
        {
            public bool acked;
            public int time;
        }

        private uint[] receivedSequenceBuffer = new uint[BUFFER_SIZE];
        private PacketData[] receivedPacketDatas = new PacketData[BUFFER_SIZE];
        public ref PacketData InsertReceivedPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            receivedSequenceBuffer[index] = sequence;
            return ref receivedPacketDatas[index];
        }
        public uint GenerateReceivedAckBits()
        {
            uint bits = 0;
            uint mask = 1;

            for (int i = 0; i < receivedPacketDatas.Length; i++)
            {
                if (receivedPacketDatas[i].acked == true)
                    bits |= mask;
                mask <<= 1;
            }

            return bits;
        }

        public void Send(bool fakeSend = false)
        {
            Packet outGoingPacket = Packet.Empty;

            outGoingPacket.Sequence = Sequence++;
            outGoingPacket.Ack = Ack;
            outGoingPacket.AckBits = GenerateReceivedAckBits();

            for (int i = SendingCommands.Count - 1; i >= 0; i--)
            {
                if ((DateTime.Now - SendingCommands[i].SendTime).TotalSeconds < 1) continue;
                if (SendingCommands[i].Size + outGoingPacket.Data.Count > MtuLimit) continue;

                SendingCommands[i].PacketId = outGoingPacket.Sequence;
                SendingCommands[i].SendTime = DateTime.Now;
                outGoingPacket.WriteCommand(SendingCommands[i]);
            }

            byte[] packet = outGoingPacket.Pack();

            if (!fakeSend)
                Socket.Send(packet, packet.Length, Ip);
        }

        public void Receive(byte[] buffer)
        {
            Packet receivedPacket = Packet.Unpack(buffer);
            if (receivedPacket == null) return;

            if (receivedPacket.Sequence > Ack)
                Ack = receivedPacket.Sequence;

            InsertReceivedPacketData(receivedPacket.Sequence).acked = true;

            for (int bit = 0; bit < BUFFER_SIZE; bit++)
                if ((receivedPacket.AckBits & (1 << bit)) != 0)
                    SendingCommands.RemoveAll(m => m.PacketId % BUFFER_SIZE == (receivedPacket.Ack - bit) % BUFFER_SIZE);

            for (int i = 0; i < receivedPacket.Messages.Count; i++)
                ReceivedCommands.Enqueue(receivedPacket.Messages[i]);
        }

        public void Send(IMessage message) => SendingCommands.Add(Command.List[Command.Lookup[message.GetType()]]);

        public static bool[] BitmaskToBoolArray(uint mask)
        {
            bool[] boolArray = new bool[32];

            for (int i = 0; i < 32; i++)
            {
                bool acked = (mask & (1 << i)) != 0;

                if (acked)
                    boolArray[i] = acked;
            }

            return boolArray;
        }
        #endregion
    }
}