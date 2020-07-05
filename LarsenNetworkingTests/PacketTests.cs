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

namespace LarsenNetworking.Tests
{
    [TestClass()]
    public class PacketTests
    {
        [TestMethod()]
        public void PackTest()
        {
            var please = NetPlayer.BitmaskToBoolArray(4042322160);

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
            //Command.Register(new IMessage[] { new PrintMessage("") });
            //Command command = new Command(new PrintMessage("PING"));
            //string changedValue = "PONG";

            //command.Fields[0].SetValue(command.Message, changedValue);
        }

        [TestMethod()]
        public void WriteCommandTest()
        {
            //Command.Register(new IMessage[] { new PrintMessage("", "", "") });
            //Command command = new Command(new PrintMessage("PING", "PONG", "BANG"));

            Packet packet = new Packet();
            //packet.WriteCommand(command);

            byte[] bytes1 = packet.Data.ToArray();

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write("PING");
            writer.Write("PONG");
            writer.Write("BANG");

            byte[] bytes2 = stream.ToArray();

        }

        //[TestMethod()]
        //public void WriteCommandWithFormaterTest()
        //{
        //    //Command.Register(new IMessage[] { new PrintMessage("", "", "") });
        //    //Command command = new Command(new PrintMessage("PING", "PONG", "BANG"));

        //    Packet packet = new Packet();

        //    byte[] byte1;
        //    byte[] byte2;

        //    var formater = new BinaryFormatter();

        //    using (var stream = new MemoryStream())
        //    {
        //        formater.Serialize(stream, "PING");
        //        formater.Serialize(stream, "PONG");
        //        formater.Serialize(stream, "BANG");

        //        byte1 = stream.ToArray();
        //    }

        //    using (var stream = new MemoryStream())
        //    {
        //        for (int i = 0; i < command.Fields.Length; i++)
        //        {
        //            formater.Serialize(stream, command.Fields[i].GetValue(command.Message));
        //        }

        //        byte2 = stream.ToArray();
        //    }
        //}
    }
}