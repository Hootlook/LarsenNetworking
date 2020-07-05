using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LarsenNetworking
{
    public class NetPlayer
    {
        enum State
        {
            Connected,
            Disconnected,
            Retrying,
        }

        public string Username { get; set; }
        public IPEndPoint Ip { get; set; }
        public UdpClient Socket { get; set; }

        public NetPlayer(IPEndPoint ip, UdpClient socket)
        {
            Socket = socket;
            Ip = ip;
        }

        #region PacketHandling
        public struct PacketData
        {
            public bool acked;
        }

        public const int BUFFER_SIZE = 32;
        public List<IMessage> Messages { get; set; } = new List<IMessage>();
        public Queue<Packet> OutPackets { get; set; } = new Queue<Packet>();
        public Queue<Packet> InPackets { get; set; } = new Queue<Packet>();
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }

        #region Mess
        private uint[] sentSequenceBuffer = new uint[BUFFER_SIZE];
        public PacketData[] sentPacketDatas = new PacketData[BUFFER_SIZE];
        public ref PacketData InsertSentPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            sentSequenceBuffer[index] = sequence;
            return ref sentPacketDatas[index];
        }

        private uint[] receivedSequenceBuffer = new uint[BUFFER_SIZE];
        private PacketData[] receivedPacketDatas = new PacketData[BUFFER_SIZE];
        public ref PacketData InsertReceivedPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            receivedSequenceBuffer[index] = sequence;
            return ref receivedPacketDatas[index];
        }
        #endregion

        public uint GenerateAckBits(PacketData[] packetDatas)
        {
            uint bits = 0;
            uint mask = 1;

            for (int i = 0; i < packetDatas.Length; ++i)
            {
                if (packetDatas[i].acked == true)
                    bits |= mask;
                mask <<= 1;
            }

            return bits;
        }

        public void Send(bool fakeSend = false)
        {
            Packet outGoingPacket = OutPackets.Count > 0 ? OutPackets.Dequeue() : Packet.Empty;

            InsertSentPacketData(Sequence).acked = false;

            AckBits = GenerateAckBits(receivedPacketDatas);

            outGoingPacket.Sequence = Sequence;
            outGoingPacket.Ack = Ack;
            outGoingPacket.AckBits = AckBits;

            byte[] packet = outGoingPacket.Pack();

            if (!fakeSend)
                Socket.Send(packet, packet.Length, Ip);

            Sequence++;
        }

        public void Receive(byte[] buffer)
        {
            Packet receivedPacket = Packet.Unpack(buffer);
            if (receivedPacket == null) return;

            if (receivedPacket.Sequence > Ack)
                Ack = receivedPacket.Sequence;

            InsertReceivedPacketData(receivedPacket.Sequence).acked = true;

            for (int i = 0; i < sentPacketDatas.Length; i++)
            {
                bool acked = (receivedPacket.AckBits & (1 << i)) != 0;

                if (acked)
                    sentPacketDatas[(receivedPacket.Ack - i) % BUFFER_SIZE].acked = true;
            }

            InPackets.Enqueue(receivedPacket);
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