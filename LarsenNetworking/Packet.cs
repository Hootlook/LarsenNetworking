using System.Collections.Generic;
using System.IO;

namespace LarsenNetworking
{
    public class Packet
    {
        public uint frame;
        public byte rpc;
        public bool ack;
        public byte id;
        public byte[] data;

        public List<byte[]> Data { get; set; }

        public static byte[] Pack(Packet data)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(data.frame);
                writer.Write(data.rpc);
                writer.Write(data.ack);
                writer.Write(data.id);

                if (data.data != null)
                    writer.Write(data.data);

                return stream.ToArray();
            }
        }
        
        public static Packet Unpack(byte[] packet)
        {
            using (var stream = new MemoryStream(packet))
            using (var reader = new BinaryReader(stream))
            {
                var s = default(Packet);

                s.frame = reader.ReadUInt32();
                s.rpc = reader.ReadByte();
                s.ack = reader.ReadBoolean();
                s.id = reader.ReadByte();
                s.data = reader.ReadBytes((int)stream.Length);

                return s;
            }
        }

        public void Add<T>(T arg)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((dynamic)arg);
                Data.Add(stream.ToArray());
            }
        }

        public void Add(long arg)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(arg);
                Data.Add(stream.ToArray());
            }
        }    
        public void Add(string arg)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(arg);
                Data.Add(stream.ToArray());
            }
        }
    }
}
