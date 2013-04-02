using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;

namespace VFSBaseTests
{
    /// <summary>
    /// Summary description for FileSystemFactoryTest
    /// </summary>
    [TestClass]
    public class FileSystemFactoryTest
    {
        const string DefaultTestfilePath = "./testfile.vhs";
        private const ulong DefaultSize = 1000 * 1000 * 1000 /* 1 MB */;

        private static FileSystemOptions InitTestFileSystem(string testfilePath, ulong size)
        {
            if (File.Exists(testfilePath)) File.Delete(testfilePath);
            var fileSystem = new FileSystemOptions(testfilePath, size);
            Assert.IsTrue(File.Exists(testfilePath), String.Format("testfile {0} should exist!", testfilePath));
            return fileSystem;
        }

        [TestMethod]
        public void TestCreateOrImport()
        {
            var fileSystemOptions = InitTestFileSystem(DefaultTestfilePath, DefaultSize);

            Assert.IsFalse(File.Exists(DefaultTestfilePath));

            var fileSystem = FileSystemFactory.CreateOrImport(fileSystemOptions);

            Assert.IsTrue(File.Exists(DefaultTestfilePath), String.Format("testfile {0} should exist!", DefaultTestfilePath));
            Assert.AreEqual(DefaultTestfilePath, fileSystemOptions.Location);
            Assert.AreEqual(DefaultSize, fileSystemOptions.DiskSize);

            FileSystemFactory.Delete(fileSystem);
        }

        [TestMethod]
        public void TestDelete()
        {
            var fileSystemOptions = InitTestFileSystem(DefaultTestfilePath, DefaultSize);

            Assert.IsTrue(File.Exists(DefaultTestfilePath));

            var fileSystem = FileSystemFactory.CreateOrImport(fileSystemOptions);

            FileSystemFactory.Delete(fileSystem);

            Assert.IsFalse(File.Exists(DefaultTestfilePath), "File should have been deleted!");
        }
    }
}
