using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class PacketHandler
    {
        public PacketHandler(UdpClient socket) => Socket = socket;

        public struct PacketData
        {
            public bool acked;
        }

        public const int BUFFER_SIZE = 32;
        public UdpClient Socket { get; set; }
        public Queue<Packet> OutGoingPackets { get; set; } = new Queue<Packet>();
        public Queue<Packet> InComingPackets { get; set; } = new Queue<Packet>();
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }

        private uint[] localSequenceBuffer = new uint[BUFFER_SIZE];
        private PacketData[] localPacketDatas = new PacketData[BUFFER_SIZE];
        public ref PacketData LocalInsertPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            localSequenceBuffer[index] = sequence;
            return ref localPacketDatas[index];
        }

        private uint[] remoteSequenceBuffer = new uint[BUFFER_SIZE];
        private PacketData[] remotePacketDatas = new PacketData[BUFFER_SIZE];
        public ref PacketData RemoteInsertPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            remoteSequenceBuffer[index] = sequence;
            return ref remotePacketDatas[index];
        }

        public void GenerateAckBits()
        {
            AckBits = 0;
            uint mask = 1;

            for (int i = 0; i < remotePacketDatas.Length; ++i)
            {
                if (remotePacketDatas[i].acked == true)
                    AckBits |= mask;
                mask <<= 1;
            }
        }

        public void Send(IPEndPoint receiver, bool fakeSend = false)
        {
            if (OutGoingPackets.Count <= 0) return;

            Packet outGoingPacket = OutGoingPackets.Dequeue();

            LocalInsertPacketData(Sequence).acked = false;

            GenerateAckBits();

            outGoingPacket.Sequence = Sequence;
            outGoingPacket.Ack = Ack;
            outGoingPacket.AckBits = AckBits;

            byte[] packet = outGoingPacket.Pack();

            if (!fakeSend)
                Socket.Send(packet, packet.Length, receiver);

            Sequence++;
        }

        public void Receive(IPEndPoint sender)
        {
            if (Socket.Available <= 0) return;

            byte[] buffer = Socket.Receive(ref sender);

            Packet receivedPacket = Packet.Unpack(buffer);
            if (receivedPacket == null) return;

            if (receivedPacket.Sequence > Ack)
                Ack = receivedPacket.Sequence;

            RemoteInsertPacketData(receivedPacket.Sequence).acked = true;

            LocalInsertPacketData(receivedPacket.Ack).acked = true;

            InComingPackets.Enqueue(receivedPacket);
        }
    }
}
