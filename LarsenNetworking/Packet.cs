using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LarsenNetworking
{
    public class PacketHandler
    {
        public class PacketData
        {
            public bool acked;
        }
        const int BUFFER_SIZE = 1024;
        public Queue<Packet> OutGoingPackets { get; set; }
        public Queue<Packet> InComingPackets { get; set; }
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }

        private uint[] sequenceBuffer = new uint[BUFFER_SIZE];

        private PacketData[] packetDatas = new PacketData[BUFFER_SIZE];

        public PacketData GetPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            if (sequenceBuffer[index] == sequence)
                return packetDatas[index];
            else
                return null;
        }

        public ref PacketData InsertPacketData(uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            sequenceBuffer[index] = sequence;
            return ref packetDatas[index];
        }

        void Send()
        {
            Packet outGoingPacket = new Packet();
            Packet mostRecentPacket = new Packet();

            InsertPacketData(Sequence).acked = false;

            outGoingPacket.Sequence = Sequence;
            outGoingPacket.Ack = mostRecentPacket.Ack;
            outGoingPacket.AckBits = AckBits;

            // Send the packet and increment the send packet sequence number

            Sequence++;
        }

        void Receive()
        {
            Packet mostRecentPacket = new Packet();
            Packet receivedPacket = new Packet();

            if (Sequence > mostRecentPacket.Sequence)
                mostRecentPacket.Sequence = Sequence;

            InsertPacketData(receivedPacket.Sequence);

            for (int i = 0; i < packetDatas.Length; i++)
                if (!packetDatas[i].acked)
                    packetDatas[i].acked = true;
        }
    }

    public class Packet
    {
        public const int mtuLimit = 1408;
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }
        public List<byte> Data { get; set; }
        public List<Rpc> Messages { get; set; }

        public byte[] Pack()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Sequence);
                writer.Write(Ack);

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
                    Packet p = new Packet();

                    p.Sequence = reader.ReadUInt16();
                    p.Ack = reader.ReadUInt16();

                    while (stream.Length > 0)
                    {
                        int messageId = reader.ReadInt32();

                        Rpc rpc = Rpc.list[messageId];

                        Type[] types = rpc.GetParameters();

                        for (int i = 0; i < types.Length; i++)
                            rpc.Values[i] = reader.Read(types[i]);

                        p.Messages.Add(rpc);
                    }

                    return p;
                }
                catch
                {
                    return null;
                }
            }
        }

        //public void WriteCommand(Command command)
        //{
        //    using (var stream = new MemoryStream())
        //    using (var writer = new BinaryWriter(stream))
        //    {
        //        writer.Write(command.Id);

        //        for (int i = 0; i < values.Length; i++)
        //        {
        //            writer.Write((dynamic)values[i]);

        //            for (int a = 0; a < stream.Length; a++)
        //                Data.Add(stream.ToArray()[a]);
        //        }
        //    }
        //}

        public void WriteRpc(Enum rpcName, object[] values)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Rpc.lookup[rpcName]);

                for (int i = 0; i < values.Length; i++)
                {
                    writer.Write((dynamic)values[i]);

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
        public static object Read(this BinaryReader reader, object type)
        {
            switch (type)
            {
                case string _:
                    return reader.ReadString();
                case long _:
                    return reader.ReadInt64();
                case ulong _:
                    return reader.ReadUInt64();
                case int _:
                    return reader.ReadInt32();
                case uint _:
                    return reader.ReadUInt32();
                case short _:
                    return reader.ReadInt16();
                case ushort _:
                    return reader.ReadUInt16();
                case byte _:
                    return reader.ReadByte();

                default:
                    return null;
            }
        }
    }

    //public static class BinaryWriterExtensions
    //{
    //    public static object Read(this BinaryWriter writer, object type)
    //    {
    //        switch (type)
    //        {
    //            case string _:
    //                return writer.Write(type as st);
    //            case long _:
    //                return writer.Write();
    //            case ulong _:
    //                return writer.Write();
    //            case int _:
    //                return writer.Write();
    //            case uint _:
    //                return writer.Write();
    //            case short _:
    //                return writer.Write();
    //            case ushort _:
    //                return writer.Write();
    //            case byte _:
    //                return writer.Write();

    //            default:
    //                return null;
    //        }
    //    }
    //}
}