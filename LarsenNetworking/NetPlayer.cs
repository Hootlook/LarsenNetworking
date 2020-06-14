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
        public Queue<Packet> OutGoingPackets { get; set; } = new Queue<Packet>();
        public Queue<Packet> InComingPackets { get; set; } = new Queue<Packet>();
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }

        private uint[] localSequenceBuffer = new uint[BUFFER_SIZE];
        public PacketData[] localPacketDatas = new PacketData[BUFFER_SIZE];
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
            if (OutGoingPackets.Count <= 0) return;

            Packet outGoingPacket = OutGoingPackets.Dequeue();

            LocalInsertPacketData(Sequence).acked = false;

            AckBits = GenerateAckBits(remotePacketDatas);

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

            RemoteInsertPacketData(receivedPacket.Sequence).acked = true;

            LocalInsertPacketData(receivedPacket.Ack).acked = true;

            if (receivedPacket.Sequence > Ack)
                Ack = receivedPacket.Sequence;

            //for (int i = Ack; i < receivedPacket.Sequence; i++)
            //{
            //    var index = i % BUFFER_SIZE;

            //    localPacketDatas[index].acked = false;
            //}

            //AckBits = GenerateAckBits(localPacketDatas);

            //for (int i = 0; i < localPacketDatas.Length; i++)
            //{
            //    int index = (receivedPacket.Ack + i) % BUFFER_SIZE;

            //    bool acked = (receivedPacket.AckBits & (1 << index - 1)) != 0;

            //    if (acked)
            //    {
            //        localPacketDatas[i].acked = true;
            //    }
            //}

            InComingPackets.Enqueue(receivedPacket);
        }
        #endregion
    }
}