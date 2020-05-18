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
        public class PacketData
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
            packetDatas[index] = new PacketData();
            return ref packetDatas[index];
        }

        public void Send(IPEndPoint receiver)
        {
            if (OutGoingPackets.Count <= 0) return;

            Packet outGoingPacket = OutGoingPackets.Dequeue();

            InsertPacketData(Sequence).acked = false;

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

            //if (receivedPacket.Sequence > Ack)
            //    Ack = receivedPacket.Sequence;

            //InsertPacketData(receivedPacket.Sequence);

            //for (int i = 0; i < packetDatas.Length; i++)
            //{
            //    if (!packetDatas[i].acked)
            //        packetDatas[i].acked = true;
            //    AckBits |= Convert.ToUInt32(packetDatas[i].acked) << i;
            //}

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
                        Sequence = reader.ReadUInt16(),
                        Ack = reader.ReadUInt16(),
                        AckBits = reader.ReadUInt32()
                    };

                    while (stream.Position != stream.Length)
                    {
                        int messageId = reader.ReadInt32();

                        Command command = Command.List[messageId];

                        for (int i = 0; i < command.Fields.Length; i++)
                            command.Fields[i].SetValue(command.Fields[i], reader.Read(command.Fields[i].FieldType));
                            //command.Fields.SetValue(reader.Read(command.Fields[i].FieldType), i);
                        //command.Values[i] = reader.Read(command.Fields[i].FieldType);

                        p.Messages.Add(command);
                    }

                    return p;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        //public static Packet Unpack(byte[] packet)
        //{
        //    using (var stream = new MemoryStream(packet))
        //    using (var reader = new BinaryReader(stream))
        //    {
        //        try
        //        {
        //            Packet p = new Packet();

        //            p.Sequence = reader.ReadUInt16();
        //            p.Ack = reader.ReadUInt16();

        //            while (stream.Length > 0)
        //            {
        //                int messageId = reader.ReadInt32();

        //                Rpc rpc = Rpc.list[messageId];

        //                Type[] types = rpc.GetParameters();

        //                for (int i = 0; i < types.Length; i++)
        //                    rpc.Values[i] = reader.Read(types[i]);

        //                p.Messages.Add(rpc);
        //            }

        //            return p;
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }
        //}

        public void WriteCommand(Command command)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(command.Id);

                for (int i = 0; i < command.Values.Length; i++)
                {
                    writer.Write((dynamic)command.Values[i]);

                    for (int a = 0; a < stream.Length; a++)
                        Data.Add(stream.ToArray()[a]);
                }
            }
        }

        //    public void WriteRpc(Enum rpcName, object[] values)
        //    {
        //        using (var stream = new MemoryStream())
        //        using (var writer = new BinaryWriter(stream))
        //        {
        //            writer.Write(Rpc.lookup[rpcName]);

        //            for (int i = 0; i < values.Length; i++)
        //            {
        //                writer.Write((dynamic)values[i]);

        //                for (int a = 0; a < stream.Length; a++)
        //                    Data.Add(stream.ToArray()[a]);
        //            }
        //        }
        //    }
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