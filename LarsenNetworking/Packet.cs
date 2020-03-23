using System.Collections.Generic;
using System.IO;

namespace LarsenNetworking
{
    public struct Packet
    {
        public uint frame;
        public byte rpc;
        public bool ack;
        public byte id;
        public byte[] data;

        const int DATA_BUFFER_CAP = 1000;
        public static byte[] Pack(Packet data)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (data.data == null)
                    data.data = new byte[1];

                writer.Write(data.frame);
                writer.Write(data.rpc);
                writer.Write(data.ack);
                writer.Write(data.id);
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
                s.data = reader.ReadBytes(DATA_BUFFER_CAP);

                return s;
            }
        }

    }
}
