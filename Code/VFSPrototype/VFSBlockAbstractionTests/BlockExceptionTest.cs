using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBlockAbstraction;

namespace VFSBlockAbstractionTests
{
    [TestClass]
    public class BlockExceptionTest
    {
        [TestMethod]
        public void TestBlockException()
        {
            var e1 = new BlockException();
            Assert.IsTrue(e1.Message.Contains ("BlockException"));

            var e2 = new BlockException("1");
            Assert.AreEqual("1", e2.Message);

            var e3 = new BlockException("2", new BlockException("3"));
            Assert.AreEqual("2", e3.Message);
            Assert.AreEqual("3", e3.InnerException.Message);

            using (var ms = new MemoryStream ()) {
                var serializer = new BinaryFormatter ();
                serializer.Serialize (ms, e3);
                ms.Seek (0, SeekOrigin.Begin);

                var e4 = serializer.Deserialize (ms) as BlockException;

                Assert.IsNotNull (e4);
                Assert.AreEqual ("2", e4.Message);
                Assert.AreEqual ("3", e4.InnerException.Message);
            }
        }
    }
}
