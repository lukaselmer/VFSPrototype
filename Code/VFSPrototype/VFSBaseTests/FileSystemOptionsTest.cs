using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;

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
                const ulong size = 1001UL;
                const uint masterBlockSize = 30000U;

                var o1 = new FileSystemOptions("", 0) { Size = size, MasterBlockSize = masterBlockSize };

                o1.Serialize(m);

                m.Seek(0, SeekOrigin.Begin);

                var o2 = FileSystemOptions.Deserialize(m);
                Assert.AreEqual(size, o2.Size);
                Assert.AreEqual(masterBlockSize, o2.MasterBlockSize);
            }
        }
    }
}
