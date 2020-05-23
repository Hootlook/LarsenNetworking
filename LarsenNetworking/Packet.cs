using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public class Packet
    {
        public static Packet Empty { get { return new Packet(); } }

        public const int mtuLimit = 1408;
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }
        public List<byte> Data { get; set; } = new List<byte>();
        public List<Command> Messages { get; set; } = new List<Command>();

        public byte[] Pack()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Sequence);
                writer.Write(Ack);
                writer.Write(AckBits);

                if (Data != null)
                    writer.Write(Data.ToArray());

                return stream.ToArray();
            }
        }

        public static Packet Unpack(byte[] packet)
        {
            using (var stream = new MemoryStream(packet))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    Packet p = new Packet
                    {
                        Ack = reader.ReadUInt16(),
                        Sequence = reader.ReadUInt16(),
                        AckBits = reader.ReadUInt32()
                    };

                    while (stream.Position != stream.Length)
                    {
                        int messageId = reader.ReadInt32();

                        Command command = Command.List[messageId];

                        for (int i = 0; i < command.Fields.Length; i++)
                            command.Fields[i].SetValue(command.Message, reader.Read(command.Fields[i].FieldType));

                        p.Messages.Add(command);
                    }

                    return p;
                }
                catch (Exception e)
                {
                    Console.WriteLine("/!\\ Malformed packet /!\\ : " + e.Message);
                    return null;
                }
            }
        }

        public void WriteCommand(Command command)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(command.Id);

                for (int i = 0; i < command.Fields.Length; i++)
                {
                    writer.Write((dynamic)command.Fields[i].GetValue(command.Message));

                    for (int a = 0; a < stream.Length; a++)
                        Data.Add(stream.ToArray()[a]);
                }
            }
        }
    }
}

namespace System.IO
{
    public static class BinaryReaderExtensions
    {
        public static object Read(this BinaryReader reader, Type type)
        {
            switch (type.Name)
            {
                case "String":
                    return reader.ReadString();
                case "Int64":
                    return reader.ReadInt64();
                case "UInt64":
                    return reader.ReadUInt64();
                case "Int32":
                    return reader.ReadInt32();
                case "UInt32":
                    return reader.ReadUInt32();
                case "Int16":
                    return reader.ReadInt16();
                case "UInt16":
                    return reader.ReadUInt16();
                case "Byte":
                    return reader.ReadByte();

                default:
                    return null;
            }
        }
    }
}