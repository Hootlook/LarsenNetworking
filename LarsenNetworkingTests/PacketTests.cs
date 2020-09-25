using Microsoft.VisualStudio.TestTools.UnitTesting;
using LarsenNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace LarsenNetworking.Tests
{
    public class PrintMessage : IMessage
    {
        public string message;

        public PrintMessage(string message)
        {
            this.message = message;
        }

        public void Execute()
        {
            PacketTests.remoteTexts.Add(message);
        }
    }

    public class ConnectionMessage : IMessage
    {
        public void Execute()
        {

        }
    }

    [TestClass()]
    public class PacketTests
    {
        public static List<string> remoteTexts = new List<string>();

        [TestMethod()]
        public void PackTest()
        {
            Packet packet = new Packet();

            Assert.Fail();
        }

        [TestMethod()]
        public void Read()
        {
            byte[] sendBytes;
            string receiveBytes;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write("Is anybody there?");
                sendBytes = stream.ToArray();
            }

            using (var stream = new MemoryStream(sendBytes))
            using (var reader = new BinaryReader(stream))
            {
                receiveBytes = (string)reader.Read(typeof(string));
            }

            Assert.IsTrue(receiveBytes == "Is anybody there?");
        }

        [TestMethod()]
        public void UnpackTest()
        {
            Command.Register(new IMessage[] {
                new ConnectionMessage(),
                new PrintMessage("")
            });

            Packet packet = new Packet();

            packet.WriteCommand(new Command(new ConnectionMessage()));
            packet.WriteCommand(new Command(new ConnectionMessage()));
            packet.WriteCommand(new Command(new ConnectionMessage()));
            packet.WriteCommand(new Command(new ConnectionMessage()));

            var buffer = packet.Pack();

            Packet packet1 = Packet.Unpack(buffer);

            bool result = true;

            foreach (var item in packet1.Messages)
                result &= item.Id == 0;

            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void WriteCommandTest()
        {
            Command.Register(new IMessage[] {
                new ConnectionMessage(),
                new PrintMessage("")
            });

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            
            Packet packet = new Packet();
            Packet packet1 = new Packet();

            packet.WriteCommand(new Command(new ConnectionMessage()));
            packet1.WriteCommand(new Command(new ConnectionMessage()));

            byte[] bytes = packet.Data.ToArray();

            writer.Write(packet1.Data.ToArray());

            byte[] bytes1 = stream.ToArray();

            Assert.AreEqual(bytes.Length, bytes1.Length);
        }



        [TestMethod()]
        public void NetworkerTest()
        {
            Command.Register(new IMessage[] {
                new ConnectionMessage(),
                new PrintMessage("")
            });

            var server = new Networker();
            var client = new Networker();

            server.Host();

            if (!client.Connect()) Assert.Fail();

            string[] texts = new []{ "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            for (int i = 0; i < texts.Length; i++)
            {
                client.Send(new PrintMessage(texts[i]), new Random().Next(1, 3) == 1);
            }

            server.Clients.TryGetValue(client.ClientIp, out Connection remoteClient);

            Thread.Sleep(10000);

            Assert.AreEqual(remoteTexts.ToArray(), texts);

        }
    }
}