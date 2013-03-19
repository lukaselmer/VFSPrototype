using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemTest
    {
        const string DefaultTestfilePath = "./testfile.vhs";
        private const long DefaultSize = 1000 * 1000 * 1000 /* 1 MB */;
        
        private static FileSystem InitTestFileSystem(string testfilePath, long size)
        {
            if (File.Exists(testfilePath)) File.Delete(testfilePath);
            var fileSystem = new FileSystem(testfilePath, size);
            Assert.IsTrue(File.Exists(testfilePath), String.Format("testfile {0} should exist!", testfilePath));
            return fileSystem;
        }

        [TestMethod]
        public void TestConstructor()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);

            Assert.IsTrue(File.Exists(DefaultTestfilePath), String.Format("testfile {0} should exist!", DefaultTestfilePath));
            Assert.AreEqual(DefaultTestfilePath, fs.Location);
            Assert.AreEqual(DefaultSize, fs.DiskSize);

            fs.Destroy();
        }

        [TestMethod]
        public void TestInvalidLocationInConstructor()
        {
            File.WriteAllText(DefaultTestfilePath, "");
            try
            {
                new FileSystem(DefaultTestfilePath, DefaultSize);
                Assert.Fail("Should throw exception");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestDestroy()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            fs.Destroy();

            Assert.IsFalse(File.Exists(DefaultTestfilePath), "File should have been deleted!");
        }
    }
}
