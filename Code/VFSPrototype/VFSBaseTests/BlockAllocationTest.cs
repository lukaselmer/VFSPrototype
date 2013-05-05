﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Persistence.Blocks;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockAllocationTest
    {
        [TestMethod]
        public void TestContinousAllocation()
        {
            var b = new BlockAllocation();
            Assert.AreEqual(2, b.Allocate());
            Assert.AreEqual(3, b.Allocate());
            Assert.AreEqual(4, b.Allocate());
            Assert.AreEqual(5, b.Allocate());
            Assert.AreEqual(6, b.Allocate());
            Assert.AreEqual(7, b.Allocate());
        }


        [TestMethod]
        public void TestSerializeAndDeserialize()
        {
            using (var m = new MemoryStream())
            {
                var b1 = new BlockAllocation();
                Assert.AreEqual(2, b1.Allocate());
                Assert.AreEqual(3, b1.Allocate());
                Assert.AreEqual(4, b1.Allocate());
                Assert.AreEqual(5, b1.Allocate());
                b1.Serialize(m);

                m.Seek(0, SeekOrigin.Begin);

                var b2 = BlockAllocation.Deserialize(m);
                Assert.AreEqual(6, b2.Allocate());
                Assert.AreEqual(7, b2.Allocate());
                Assert.AreEqual(8, b2.Allocate());
            }
        }
    }
}
