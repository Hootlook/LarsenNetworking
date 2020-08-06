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

        public bool PacketExist(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            if (sequenceBuffer[index] == sequence)
                return true;
            else
                return false;
        }

        public uint GenerateAckBits()
        {
            uint bits = 0;
            uint mask = 1;

            for (int i = 0; i < packetDatas.Length; i++)
            {
                uint sequence = sequenceBuffer[i];
                bool acked = packetDatas[i].acked;

                if (acked && (sequence >= Ack - BUFFER_SIZE && sequence <= Ack))
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
            outGoingPacket.AckBits = GenerateAckBits();

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

            if (receivedPacket.IsNewerThan(Ack))
            {
                Ack = receivedPacket.Sequence;
            }
            else
            {
                if (PacketExist(Ack))
                    return;
                if (GetPacketData(Ack).HasValue)
                    if (GetPacketData(Ack).Value.acked)
                        return;
            }

            InsertPacketData(receivedPacket.Sequence).acked = true;

            for (int bit = 0; bit < BUFFER_SIZE; bit++)
                if ((receivedPacket.AckBits & (1 << bit)) != 0)
                    SendingCommands.RemoveAll(m => m.PacketId == receivedPacket.Ack - bit);

            for (int i = 0; i < receivedPacket.Messages.Count; i++)
                ReceivedCommands.Enqueue(receivedPacket.Messages[i]);
        }

        public void Send(IMessage message) => SendingCommands.Add(new Command(message));

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

        public static string BitmaskToString(uint mask)
        {
            string stringRep = "";

            for (int i = 0; i < 32; i++)
                stringRep += (mask & (1 << i)) != 0 ? "1" : "0";

            return stringRep;
        }
        #endregion
    }
}