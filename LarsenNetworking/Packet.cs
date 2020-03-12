using System.IO;

namespace LarsenNetworking
{
    public static class Packet
    {
        const int DATA_BUFFER_CAP = 1000; 
        public static byte[] Pack(Data data)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            data.data = new byte[DATA_BUFFER_CAP];

            writer.Write(data.request);
            writer.Write(data.frame);
            writer.Write(data.ack);
            writer.Write(data.id);
            writer.Write(data.data);

            return stream.ToArray();
        }

        public static Data Unpack(byte[] packet)
        {
            var reader = new BinaryReader(new MemoryStream(packet));

            var s = default(Data);

            s.request = reader.ReadByte();
            s.frame = reader.ReadUInt32();
            s.ack = reader.ReadBoolean();
            s.id = reader.ReadByte();
            s.data = reader.ReadBytes(DATA_BUFFER_CAP);

            return s;
        }
    }

    public struct Data
    {
        public byte request;
        public uint frame;
        public bool ack;
        public byte id;
        public byte[] data;
    }
}
