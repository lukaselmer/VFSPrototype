using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Persistence;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemOptionsTest
    {
        [TestMethod]
        public void TestSerializeAndDeserialize()
        {
            using (var m = new MemoryStream())
            {
                const long size = 1001L;
                const uint masterBlockSize = 30000U;

                var o1 = new FileSystemOptions("", 0) { DiskSize = size, MasterBlockSize = masterBlockSize };

                o1.Serialize(m);

                m.Seek(0, SeekOrigin.Begin);

                var o2 = FileSystemOptions.Deserialize(m);
                Assert.AreEqual(size, o2.DiskSize);
                Assert.AreEqual(masterBlockSize, o2.MasterBlockSize);
            }
        }

        [TestMethod]
        public void TestSerializeAndDeserializeBlockAllocationInOptions()
        {
            using (var m = new MemoryStream())
            {
                const long size = 1001L;
                const uint masterBlockSize = 30000U;

                var o1 = new FileSystemOptions("", 0) { DiskSize = size, MasterBlockSize = masterBlockSize };
                var b1 = o1.BlockAllocation;
                Assert.AreEqual(1, b1.Allocate());
                Assert.AreEqual(2, b1.Allocate());
                Assert.AreEqual(3, b1.Allocate());
                Assert.AreEqual(4, b1.Allocate());
                b1.Free(33);
                b1.Free(42);

                o1.Serialize(m);

                m.Seek(0, SeekOrigin.Begin);

                var o2 = FileSystemOptions.Deserialize(m);
                var b2 = o2.BlockAllocation;
                Assert.AreEqual(42, b2.Allocate());
                Assert.AreEqual(33, b2.Allocate());
                Assert.AreEqual(5, b2.Allocate());
                Assert.AreEqual(6, b2.Allocate());
                Assert.AreEqual(7, b2.Allocate());
            }
        }

        [TestMethod]
        public void TestMaximumFileSize()
        {
            const long size = 1001L;
            const uint masterBlockSize = 30000U;
            var o = new FileSystemOptions("", 0) { DiskSize = size, MasterBlockSize = masterBlockSize };
            o.BlockSize = (int)MathUtil.KB(4);
            Assert.AreEqual(MathUtil.GB(1), o.MaximumFileSize);
            o.BlockSize = (int)MathUtil.KB(8);
            Assert.AreEqual(MathUtil.GB(16), o.MaximumFileSize);
            o.BlockSize = (int)MathUtil.KB(16);
            Assert.AreEqual(MathUtil.GB(256), o.MaximumFileSize);
            o.BlockSize = (int)MathUtil.KB(32);
            Assert.AreEqual(MathUtil.GB(4096), o.MaximumFileSize);
        }

        [ExpectedException(typeof(VFSException))]
        [TestMethod]
        public void TestSetInvlidBlockSize()
        {
            var o = new FileSystemOptions("", 0);
            o.BlockSize = (int)MathUtil.KB(1);
        }

        [TestMethod]
        public void TestStreamCodingStrategyInitialization()
        {
            var o1 = new FileSystemOptions("", 0);

            using (var ms = new MemoryStream())
            {
                var b = new BinaryFormatter();
                b.Serialize(ms, o1);

                ms.Seek(0, SeekOrigin.Begin);

                var o2 = b.Deserialize(ms) as FileSystemOptions;

                Assert.IsNotNull(o2);

                var streamCodingStrategy = o2.StreamCodingStrategy;
                Assert.IsNotNull(streamCodingStrategy);
                Assert.AreNotSame(o1.StreamCodingStrategy, streamCodingStrategy);
            }

            
        }


    }
}
