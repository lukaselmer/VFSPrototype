using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;

namespace VFSBaseTests
{
    [TestClass]
    public class VFSFileStreamTest
    {
        private static VFSFileStream VFSFileStream()
        {
            return new VFSFileStream(new VFSFile("test"), null, new FileSystemOptions("", 0), null, null, null);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestSeek()
        {
            var s = VFSFileStream();
            Assert.AreEqual(false, s.CanSeek);
            s.Seek(0, SeekOrigin.Begin);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestSetLength()
        {
            VFSFileStream().SetLength(0);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestLength()
        {
            Assert.AreEqual(0, VFSFileStream().Length);
        }
    }
}
