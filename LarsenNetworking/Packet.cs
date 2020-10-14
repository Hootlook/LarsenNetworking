using System;
using System.Collections.Generic;
using System.IO;

namespace LarsenNetworking
{
    public class Packet
    {
        public const string PROTOCOL_ID = "LarsenNetworking";

        public static Packet Empty { get { return new Packet(); }  }
        public ushort Sequence { get; set; }
        public ushort Ack { get; set; }
        public uint AckBits { get; set; }
        public List<byte> Data { get; set; } = new List<byte>();
        public List<Command> Commands { get; set; } = new List<Command>();

        public byte[] Pack()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(PROTOCOL_ID);
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
                    if (reader.ReadString() != PROTOCOL_ID) throw new Exception();

                    Packet packet = new Packet
                    {
                        Sequence = reader.ReadUInt16(),
                        Ack = reader.ReadUInt16(),
                        AckBits = reader.ReadUInt32()
                    };

                    while (stream.Position != stream.Length)
                    {
                        Command command = Command.List[reader.ReadInt32()].Clone();

                        if (command.Method != Command.SendingMethod.Reliable)
                            command.OrderId = reader.ReadUInt16();

                        for (int i = 0; i < command.Fields.Length; i++)
                            command.Fields[i].SetValue(command, reader.Read(command.Fields[i].FieldType));

                        packet.Commands.Add(command);
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

                if (command.Method != Command.SendingMethod.Reliable)
                    writer.Write(command.OrderId);

                for (int i = 0; i < command.Fields.Length; i++)
                    writer.Write(command.Fields[i].GetValue(command));

                byte[] buffer = stream.ToArray();

                for (int a = 0; a < buffer.Length; a++)
                    Data.Add(buffer[a]);

                return buffer.Length;
            }
        }

        public bool IsNewerThan(ushort sequence)
        {
            return ((Sequence > sequence) && (Sequence - sequence <= 32768)) ||
                   ((Sequence < sequence) && (sequence - Sequence > 32768));
        }
    }
}

namespace System.IO
{
    public static class BinaryReaderExtensions
    {
        public static object Read(this BinaryReader reader, Type type)
        {
            string typeName = type.Name;

        Retry:

            switch (typeName)
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
                case "Enum":
                    return reader.ReadInt32();
            }

            if (typeName == type.BaseType.Name)
                throw new ArgumentException("Can't read this type");

            typeName = type.BaseType.Name;

            goto Retry;
        }
    }

    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, object obj)
        {
            string typeName = obj.GetType().Name;

        Retry:

            switch (typeName)
            {
                case "String":
                    writer.Write((string)obj);
                    return;
                case "Int64":
                    writer.Write((long)obj);
                    return;
                case "UInt64":
                    writer.Write((ulong)obj);
                    return;
                case "Int32":
                    writer.Write((int)obj);
                    return;
                case "UInt32":
                    writer.Write((uint)obj);
                    return;
                case "Int16":
                    writer.Write((short)obj);
                    return;
                case "UInt16":
                    writer.Write((ushort)obj);
                    return;
                case "Byte":
                    writer.Write((byte)obj);
                    return;
                case "Enum":
                    writer.Write((int)obj);
                    return;
            }

            if (typeName == obj.GetType().BaseType.Name)
                throw new ArgumentException("Can't write this type");

            typeName = obj.GetType().BaseType.Name;

            goto Retry;
        }
    }
}