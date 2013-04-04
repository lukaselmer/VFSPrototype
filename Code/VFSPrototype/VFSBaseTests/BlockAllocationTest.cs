using System.IO;
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


        [TestMethod]
        public void TestSerializeAndDeserialize()
        {
            using (var m = new MemoryStream())
            {
                const long size = 1001L;
                const uint masterBlockSize = 30000U;

                var b1 = new BlockAllocation();
                Assert.AreEqual(1, b1.Allocate());
                Assert.AreEqual(2, b1.Allocate());
                Assert.AreEqual(3, b1.Allocate());
                Assert.AreEqual(4, b1.Allocate());
                b1.Free(33);
                b1.Free(42);
                b1.Serialize(m);

                m.Seek(0, SeekOrigin.Begin);

                var b2 = BlockAllocation.Deserialize(m);
                Assert.AreEqual(42, b2.Allocate());
                Assert.AreEqual(33, b2.Allocate());
                Assert.AreEqual(5, b2.Allocate());
                Assert.AreEqual(6, b2.Allocate());
                Assert.AreEqual(7, b2.Allocate());
            }
        }
    }
}
