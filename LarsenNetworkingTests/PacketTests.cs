using Microsoft.VisualStudio.TestTools.UnitTesting;
using LarsenNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LarsenNetworking.Tests
{
    [TestClass()]
    public class PacketTests
    {
        [TestMethod()]
        public void PackTest()
        {
            Packet packet = new Packet();

            Command.Register(new IMessage[] {
                new ChallengeConnectMessage("help"),
                new ConnectMessage()
            });


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
            Command.Register(new IMessage[] { new PrintMessage("") });
            Command command = new Command(new PrintMessage("PING"));
            string changedValue = "PONG";

            command.Fields[0].SetValue(command, changedValue);


            Assert.Fail();
        }

        [TestMethod()]
        public void WriteRpcTest()
        {
            Assert.Fail();
        }
    }
}