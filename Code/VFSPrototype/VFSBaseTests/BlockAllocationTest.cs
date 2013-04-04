using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockAllocationTest
    {
        [TestMethod]
        public void TestContinousAllocation()
        {
            var b = new BlockAllocation();
            Assert.AreEqual(1, b.Allocate());
            Assert.AreEqual(2, b.Allocate());
            Assert.AreEqual(3, b.Allocate());
            Assert.AreEqual(4, b.Allocate());
            Assert.AreEqual(5, b.Allocate());
            Assert.AreEqual(6, b.Allocate());
        }

        [TestMethod]
        public void TestFreeAndAllocate()
        {
            var b = new BlockAllocation();
            b.Free(22);
            b.Free(20);
            b.Free(42);
            Assert.AreEqual(42, b.Allocate());
            Assert.AreEqual(20, b.Allocate());
            Assert.AreEqual(22, b.Allocate());
        }

        [TestMethod]
        public void TestFreeAndAllocateMixed()
        {
            var b = new BlockAllocation();
            Assert.AreEqual(1, b.Allocate());
            Assert.AreEqual(2, b.Allocate());
            b.Free(22);
            b.Free(20);
            b.Free(42);
            Assert.AreEqual(42, b.Allocate());
            Assert.AreEqual(20, b.Allocate());
            Assert.AreEqual(22, b.Allocate());
            Assert.AreEqual(3, b.Allocate());
            b.Free(20);
            b.Free(42);
            Assert.AreEqual(42, b.Allocate());
            Assert.AreEqual(20, b.Allocate());
            Assert.AreEqual(4, b.Allocate());
            b.Free(42);
            b.Free(20);
            Assert.AreEqual(20, b.Allocate());
            Assert.AreEqual(42, b.Allocate());
            Assert.AreEqual(5, b.Allocate());
            b.Free(20);
            Assert.AreEqual(20, b.Allocate());
            Assert.AreEqual(6, b.Allocate());
        }
    }
}
