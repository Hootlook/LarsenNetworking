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
            Command.Initialize();

            Packet packet = new Packet();

            var cmd = new PlayerMove()
            {

            };

            cmd.Send(Method.Unreliable);

            var g = Command.List;

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