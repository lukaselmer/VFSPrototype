using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;

namespace VFSBaseTests.Exceptions
{
    [TestClass]
    public class ExceptionsTest
    {
        [TestMethod]
        public void TestAlreadyExistsException()
        {
            var e1 = new AlreadyExistsException();
            Assert.IsTrue(e1.Message.Contains("AlreadyExistsException"));

            var e2 = new AlreadyExistsException("a");
            Assert.AreEqual("a", e2.Message);

            var e3 = new AlreadyExistsException("a", new Exception("x"));
            Assert.AreEqual("a", e3.Message);
            Assert.AreEqual("x", e3.InnerException.Message);

            using (var ms = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(ms, e3);
                ms.Seek(0, SeekOrigin.Begin);

                var e4 = serializer.Deserialize(ms) as AlreadyExistsException;

                Assert.IsNotNull(e4);
                Assert.AreEqual("a", e4.Message);
                Assert.AreEqual("x", e4.InnerException.Message);
            }
        }

        [TestMethod]
        public void TestNotFoundException()
        {
            var e1 = new NotFoundException();
            Assert.IsTrue(e1.Message.Contains("NotFoundException"));

            var e2 = new NotFoundException("a");
            Assert.AreEqual("a", e2.Message);

            var e3 = new NotFoundException("a", new Exception("x"));
            Assert.AreEqual("a", e3.Message);
            Assert.AreEqual("x", e3.InnerException.Message);

            using (var ms = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(ms, e3);
                ms.Seek(0, SeekOrigin.Begin);

                var e4 = serializer.Deserialize(ms) as NotFoundException;

                Assert.IsNotNull(e4);
                Assert.AreEqual("a", e4.Message);
                Assert.AreEqual("x", e4.InnerException.Message);
            }
        }

        [TestMethod]
        public void TestVFSException()
        {
            var e1 = new VFSException();
            Assert.IsTrue(e1.Message.Contains("VFSException"));

            var e2 = new VFSException("a");
            Assert.AreEqual("a", e2.Message);

            var e3 = new VFSException("a", new Exception("x"));
            Assert.AreEqual("a", e3.Message);
            Assert.AreEqual("x", e3.InnerException.Message);

            using (var ms = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(ms, e3);
                ms.Seek(0, SeekOrigin.Begin);

                var e4 = serializer.Deserialize(ms) as VFSException;

                Assert.IsNotNull(e4);
                Assert.AreEqual("a", e4.Message);
                Assert.AreEqual("x", e4.InnerException.Message);
            }
        }
    }
}
