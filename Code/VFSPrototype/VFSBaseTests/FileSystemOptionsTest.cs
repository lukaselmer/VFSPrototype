using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Persistence;
using VFSBase.Persistence.Coding.General;
using VFSBaseTests.Helpers;

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
                const int masterBlockSize = 30000;

                var o1 = TestHelper.CreateFileSystemOptions("");
                o1.MasterBlockSize = masterBlockSize;

                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(m, o1);

                m.Seek(0, SeekOrigin.Begin);

                var o2 = FileSystemOptions.Deserialize(m, "");
                Assert.AreEqual(masterBlockSize, o2.MasterBlockSize);
            }
        }

        [TestMethod]
        public void TestSerializeAndDeserializeBlockAllocationInOptions()
        {
            using (var m = new MemoryStream())
            {
                const int masterBlockSize = 30000;

                var o1 = TestHelper.CreateFileSystemOptions("");
                o1.MasterBlockSize = masterBlockSize;
                var b1 = o1.BlockAllocation;
                Assert.AreEqual(2, b1.Allocate());
                Assert.AreEqual(3, b1.Allocate());
                Assert.AreEqual(4, b1.Allocate());
                Assert.AreEqual(5, b1.Allocate());

                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(m, o1);

                m.Seek(0, SeekOrigin.Begin);

                var o2 = FileSystemOptions.Deserialize(m, "");
                var b2 = o2.BlockAllocation;
                Assert.AreEqual(6, b2.Allocate());
                Assert.AreEqual(7, b2.Allocate());
                Assert.AreEqual(8, b2.Allocate());
            }
        }

        [TestMethod]
        public void TestMaximumFileSize()
        {
            const int masterBlockSize = 30000;
            var o = TestHelper.CreateFileSystemOptions("");
            o.MasterBlockSize = masterBlockSize;
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
            var o = TestHelper.CreateFileSystemOptions("");
            o.BlockSize = (int)MathUtil.KB(1);
        }

        [TestMethod]
        public void TestStreamCodingStrategyInitialization()
        {
            var o1 = TestHelper.CreateFileSystemOptions("");

            using (var ms = new MemoryStream())
            {
                var b = new BinaryFormatter();
                b.Serialize(ms, o1);

                ms.Seek(0, SeekOrigin.Begin);

                var o2 = b.Deserialize(ms) as FileSystemOptions;
                Assert.IsNotNull(o2);
                o2.InitializeStreamCodingStrategy("");

                Assert.IsNotNull(o2);

                var streamCodingStrategy = o2.StreamCodingStrategy;
                Assert.IsNotNull(streamCodingStrategy);
                Assert.AreNotSame(o1.StreamCodingStrategy, streamCodingStrategy);
            }
        }

        [TestMethod]
        public void TestEncryptionKey()
        {
            var o = TestHelper.CreateFileSystemOptions("");
            o.EncryptionKey = new byte[] { 1, 2, 3 };
            Assert.AreEqual(1, o.EncryptionKey[0]);
            Assert.AreEqual(2, o.EncryptionKey[1]);
            Assert.AreEqual(3, o.EncryptionKey[2]);
        }

        [TestMethod]
        public void TestApplyEncryptionSettings()
        {
            var o = TestHelper.CreateFileSystemOptions("");
            var o2 = new FileSystemOptions(o.Location, StreamEncryptionType.None, StreamCompressionType.None);
            o.EncryptionKey = new byte[] { 1, 2, 3 };
            Assert.IsNotNull(o.EncryptionKey);
            Assert.IsNull(o2.EncryptionKey);
            o2.ApplyEncryptionSettings(o);
            Assert.AreEqual(o.EncryptionKey, o2.EncryptionKey);
            Assert.AreEqual(1, o2.EncryptionKey[0]);
            Assert.AreEqual(2, o2.EncryptionKey[1]);
            Assert.AreEqual(3, o2.EncryptionKey[2]);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestApplyEncryptionSettingFail()
        {
            var o2 = new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.None);
            o2.ApplyEncryptionSettings(null);
        }

        [TestMethod]
        public void TestInitializePasswordNoneStrategy()
        {
            var o = TestHelper.CreateFileSystemOptions("");
            Assert.AreEqual(StreamEncryptionType.None, o.Encryption);
            o.InitializePassword("test");
        }

        [TestMethod]
        public void TestInitializePassword()
        {
            var o = TestHelper.CreateFileSystemOptions("");
            o.Encryption = StreamEncryptionType.SelfMadeAes;
            Assert.AreEqual(StreamEncryptionType.SelfMadeAes, o.Encryption);

            Assert.IsNull(o.EncryptionInitializationVector);
            Assert.IsNull(o.EncryptionKey);

            o.InitializePassword("test");

            Assert.IsNotNull(o.EncryptionInitializationVector);
            Assert.IsNotNull(o.EncryptionKey);
        }


    }
}
