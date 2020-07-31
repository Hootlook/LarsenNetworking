using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LarsenNetworking
{
    public class Packet
    {
        public static Packet Empty { get { return new Packet(); }  }
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

                writer.Write(Data.ToArray());

                return stream.ToArray();
            }
        }

        public static Packet Unpack(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    Packet packet = new Packet
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
                            command.Fields[i].SetValue(command.Message, reader.Read(command.Fields[i].FieldType));

                        packet.Messages.Add(command);
                    }

                    return packet;
                }
                catch (Exception e)
                {
                    Console.WriteLine("/!\\ Malformed packet /!\\ : " + e.Message);
                    return null;
                }
            }
        }

        public int WriteCommand(Command command)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(command.Id);

                for (int i = 0; i < command.Fields.Length; i++)
                    writer.Write((dynamic)command.Fields[i].GetValue(command.Message));

                byte[] buffer = stream.ToArray();

                for (int a = 0; a < buffer.Length; a++)
                    Data.Add(buffer[a]);

                return buffer.Length;
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