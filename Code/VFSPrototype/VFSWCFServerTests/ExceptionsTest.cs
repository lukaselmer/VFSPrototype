using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBlockAbstraction;
using VFSWCFService.Persistence;

namespace VFSWCFServiceTests
{
    [TestClass]
    public class ExceptionsTest
    {
        [TestMethod]
        public void TestPersistenceException()
        {
            var e1 = new PersistenceException();
            Assert.IsTrue(e1.Message.Contains("PersistenceException"));

            var e2 = new PersistenceException("a");
            Assert.AreEqual("a", e2.Message);

            var e3 = new PersistenceException("a", new Exception("x"));
            Assert.AreEqual("a", e3.Message);
            Assert.AreEqual("x", e3.InnerException.Message);

            using (var ms = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(ms, e3);
                ms.Seek(0, SeekOrigin.Begin);

                var e4 = serializer.Deserialize(ms) as PersistenceException;

                Assert.IsNotNull(e4);
                Assert.AreEqual("a", e4.Message);
                Assert.AreEqual("x", e4.InnerException.Message);
            }

        }
    }
}
