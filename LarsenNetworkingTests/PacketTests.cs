using Microsoft.VisualStudio.TestTools.UnitTesting;
using LarsenNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void UnpackTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void WriteRpcTest()
        {
            Assert.Fail();
        }
    }
}