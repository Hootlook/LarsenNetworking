using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class PacketHandler
    {
        public PacketHandler(UdpClient socket)
        {
            Socket = socket;
        }

        public struct PacketData
        {
            public bool acked;
        }

        const int BUFFER_SIZE = 1024;
        public UdpClient Socket { get; set; }
        public Queue<Packet> OutGoingPackets { get; set; } = new Queue<Packet>();
        public Queue<Packet> InComingPackets { get; set; } = new Queue<Packet>();
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }

        private uint[] sequenceBuffer = new uint[BUFFER_SIZE];

        private PacketData[] packetDatas = new PacketData[BUFFER_SIZE];

        public ref PacketData InsertPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            sequenceBuffer[index] = sequence;
            return ref packetDatas[index];
        }

        public bool PacketDataExist(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            if (sequenceBuffer[index] == sequence)
                return true;
            else
                return false;
        }

        public void GenerateAckBits()
        {
            AckBits = 0;
            uint mask = 1;

            for (int i = 0; i < packetDatas.Length; ++i)
            {
                uint sequence = Ack - ((uint)i);
                if (PacketDataExist(sequence))
                    AckBits |= mask;
                mask <<= 1;
            }
        }

        public void Send(IPEndPoint receiver)
        {
            if (OutGoingPackets.Count <= 0) return;

            Packet outGoingPacket = OutGoingPackets.Dequeue();

            InsertPacketData(Sequence).acked = true;

            GenerateAckBits();

            outGoingPacket.Sequence = Sequence;
            outGoingPacket.Ack = Ack;
            outGoingPacket.AckBits = AckBits;

            byte[] packet = outGoingPacket.Pack();

            Socket.Send(packet, packet.Length, receiver);

            Sequence++;
        }

        public void Receive(IPEndPoint sender)
        {
            if (Socket.Available <= 0) return;

            byte[] buffer = Socket.Receive(ref sender);

            Packet receivedPacket = Packet.Unpack(buffer);
            if (receivedPacket == null) return;

            if (Sequence > receivedPacket.Ack)
                Ack = receivedPacket.Ack;

            InsertPacketData(receivedPacket.Sequence);

            for (int i = 0; i < packetDatas.Length; i++)
                if (!packetDatas[i].acked)
                    packetDatas[i].acked = true;
            
            InComingPackets.Enqueue(receivedPacket);
        }
    }
}
