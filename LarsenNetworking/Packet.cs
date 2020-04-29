using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LarsenNetworking
{
    public class Packet
    {
        public uint SequenceNumber;
        public uint lastRemoteSeq;
        public byte ackMask;
        public List<byte> Data { get; set; }

        public byte[] Pack()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(SequenceNumber);
                writer.Write(lastRemoteSeq);
                writer.Write(ackMask);

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
                var s = default(Packet);

                s.SequenceNumber = reader.ReadUInt32();
                s.lastRemoteSeq = reader.ReadUInt32();
                s.ackMask = reader.ReadByte();

                for (int a = 0; a < stream.Length; a++)
                    s.Data.Add(stream.ToArray()[a]);

                return s;
            }
        }

        public void WriteMessage(byte id, object[] fields)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(id);

                for (int i = 0; i < fields.Length; i++)
                {
                    writer.Write((dynamic)fields[i]);

                    for (int a = 0; a < stream.Length; a++)
                        Data.Add(stream.ToArray()[a]);
                }
            }
        }
    }
}
