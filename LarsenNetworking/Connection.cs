using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static LarsenNetworking.Command;

namespace LarsenNetworking
{
    public class Connection
    {
        public IPEndPoint Ip { get; set; }
        public Networker Networker { get; set; }
        public long LastPing { get; set; }
        public long Ping { get; set; }
        public ConnectionState State { get; set; }

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        public Connection(IPEndPoint ip, Networker networker)
        {
            Networker = networker;
            Ip = ip;
        }

        #region PacketHandling
        private object CommandsLock { get; set; } = new object();
        public List<Command> ReceivedCommands { get; set; } = new List<Command>();
        public List<Command> SendingCommands { get; set; } = new List<Command>();
        public int MtuLimit { get; set; } = MTU_LIMIT;
        public ushort Sequence { get; set; } 
        public ushort Ack { get; set; }

        public ushort RemoteReliableOrderId { get; set; }
        public ushort LocalReliableOrderId { get; set; }
        public ushort RemoteUnreliableOrderId { get; set; }

        public const int BUFFER_SIZE = 32;
        public const int MTU_LIMIT = 1408;
        
        private PacketInfo[] localPacketData = new PacketInfo[BUFFER_SIZE];
        private PacketInfo[] remotePacketData = new PacketInfo[BUFFER_SIZE];

        public struct PacketInfo
        {
            public uint sequence;
            public bool acked;
            public long time;
        }
        
        public ref PacketInfo InsertPacketData(PacketInfo[] packetDatas, uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            packetDatas[index].sequence = sequence;
            return ref packetDatas[index];
        }

        public PacketInfo? GetPacketData(PacketInfo[] packetDatas, uint sequence)
        {
            uint index = sequence % BUFFER_SIZE;
            return packetDatas[index].sequence == sequence ? (PacketInfo?)packetDatas[index] : null;
        }

        public bool PacketDataAlreadyExist(PacketInfo[] packetDatas, uint sequence)
        {
            return packetDatas[sequence % BUFFER_SIZE].sequence == sequence;
        }

        public uint GenerateAckBits()
        {
            uint bits = 0;
            uint mask = 1;

            for (uint i = 0; i < remotePacketData.Length; i++)
            {
                uint index = (Ack - i) % BUFFER_SIZE;

                uint sequence = remotePacketData[index].sequence;
                bool acked = remotePacketData[index].acked;

                if (acked && (sequence >= Ack - BUFFER_SIZE && sequence <= Ack))
                    bits |= mask;
                mask <<= 1;
            }

            return bits;
        }

        public void Receive(byte[] buffer)
        {
            lock (CommandsLock)
            {
                Packet receivedPacket = Packet.TryUnpack(buffer);
                if (receivedPacket == null) return;

                if (receivedPacket.IsNewerThan(Ack))
                    Ack = receivedPacket.Sequence;

                else if (PacketDataAlreadyExist(remotePacketData, Ack))
                    return;

                InsertPacketData(remotePacketData, receivedPacket.Sequence).acked = true;

                PacketInfo? data = GetPacketData(localPacketData, receivedPacket.Ack);

                if (data != null)
                    Ping += (long)((Networker.Time.ElapsedMilliseconds - data.Value.time - Ping) * 0.1);

                for (int bit = 0; bit < localPacketData.Length; bit++)
                    if ((receivedPacket.AckBits & (1 << bit)) != 0)
                        SendingCommands.RemoveAll(m => m.PacketId == receivedPacket.Ack - bit);

                for (int i = 0; i < receivedPacket.Commands.Count; i++)
                    ReceivedCommands.Add(receivedPacket.Commands[i]);
            }
        }

        public void Send(Command command = null, bool fakeSend = false)
        {
            lock (CommandsLock)
            {
                Packet outGoingPacket = Packet.Empty;

                outGoingPacket.Sequence = Sequence++;
                outGoingPacket.Ack = Ack;
                outGoingPacket.AckBits = GenerateAckBits();

                if (command != null)
                {
                    if (command.Method == SendingMethod.Unreliable)
                    {
                        command.OrderId = RemoteUnreliableOrderId++;
                        outGoingPacket.Data.AddRange(command.GetBytes());
                    }

                    if (command.Method == SendingMethod.ReliableOrdered)
                        command.OrderId = RemoteReliableOrderId++;

                    if (command.Method != SendingMethod.Unreliable)
                        SendingCommands.Add(command);
                }

                for (int i = SendingCommands.Count - 1; i >= 0; i--)
                {
                    if ((DateTime.Now - SendingCommands[i].SendTime).TotalMilliseconds < 100) continue;
                    if (SendingCommands[i].Size + outGoingPacket.Data.Count > MtuLimit) continue;

                    SendingCommands[i].PacketId = outGoingPacket.Sequence;
                    SendingCommands[i].SendTime = DateTime.Now;
                    outGoingPacket.Data.AddRange(SendingCommands[i].GetBytes());
                }

                InsertPacketData(localPacketData, outGoingPacket.Sequence).time = Networker.Time.ElapsedMilliseconds;

                byte[] packet = outGoingPacket.Pack();

                if(!fakeSend)
                    Networker.Socket.Send(packet, packet.Length, Ip);
            }
        }

        public void Update()
        {
            lock (CommandsLock)
            {
                if (ReceivedCommands.Count > 0)
                {
                    var ReliableOrdered = ReceivedCommands.Where(c => c.Method == SendingMethod.ReliableOrdered).OrderBy(c => c.OrderId).ToList();
                    var Unreliable = ReceivedCommands.Where(c => c.Method == SendingMethod.Unreliable).OrderByDescending(c => c.OrderId).LastOrDefault();
                    var Reliable = ReceivedCommands.Where(c => c.Method == SendingMethod.Reliable).ToList();

                    foreach (var cmd in ReliableOrdered.ToArray())
                    {
                        if (cmd.OrderId == LocalReliableOrderId)
                        {
                            cmd.Action();
                            ReliableOrdered.Remove(cmd);
                            LocalReliableOrderId++;
                        }
                    }

                    Reliable.ForEach(c => c.Action());
                    Unreliable?.Action();

                    ReceivedCommands = ReliableOrdered;
                }
            }
        }
        #endregion
    }
}