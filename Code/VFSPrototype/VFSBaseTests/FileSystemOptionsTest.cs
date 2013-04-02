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
                var o1 = new FileSystemOptions("", 0);

                o1.Size = 1001;
                o1.MasterBlockSize = 30000;
                o1.Serialize(m);

                m.Seek(0, SeekOrigin.Begin);

                var o2 = FileSystemOptions.Deserialize(m);
                Assert.AreEqual(1001UL, o2.Size);
                Assert.AreEqual(30000U, o2.MasterBlockSize);
            }
        }
    }
}
