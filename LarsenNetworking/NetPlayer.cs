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
        public List<Command> OutCommands { get; set; } = new List<Command>();
        public Queue<Packet> OutPackets { get; set; } = new Queue<Packet>();
        public Queue<Packet> InPackets { get; set; } = new Queue<Packet>();
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

        public void Send(bool fakeSend = false)
        {
            Packet outGoingPacket = Packet.Empty;

            outGoingPacket.Sequence = Sequence++;
            outGoingPacket.Ack = Ack;
            outGoingPacket.AckBits = GenerateReceivedAckBits();

            for (int i = OutCommands.Count - 1; i >= 0; i--)
            {
                Command currentCommand = OutCommands[i];

                if ((currentCommand.SendTime - DateTime.Now).TotalSeconds > 1) continue;
                if (currentCommand.Size + outGoingPacket.Data.Count > MtuLimit) continue;

                currentCommand.PacketId = outGoingPacket.Sequence;
                currentCommand.SendTime = DateTime.Now;
                outGoingPacket.WriteCommand(currentCommand);
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
            {
                bool acked = (receivedPacket.AckBits & (1 << bit)) != 0;

                if (acked)
                    OutCommands.RemoveAll(m => m.PacketId == (receivedPacket.Ack - bit) % BUFFER_SIZE);
            }

            InPackets.Enqueue(receivedPacket);
        }

        public void Send(IMessage message) => OutCommands.Add(Command.List[Command.Lookup[message.GetType()]]);

        public uint GenerateReceivedAckBits()
        {
            uint bits = 0;
            uint mask = 1;
            uint lastSeq = 0;

            for (int i = 0; i < receivedPacketDatas.Length; i++)
            {
                uint currentSequenceNumber = receivedSequenceBuffer[i];
                uint lastSequenceNumber = receivedSequenceBuffer[lastSeq++];

                if (receivedPacketDatas[i].acked == true && currentSequenceNumber == lastSequenceNumber + 1)
                    bits |= mask;

                mask <<= 1;
            }

            return bits;
        }

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