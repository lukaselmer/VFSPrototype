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
        const string DefaultTestfilePath = "../../../Testfiles/testfile.vhs";
        const string ExampleTestfilePath = "../../../Testfiles/example.vhs";
        private const ulong DefaultSize = 1000 * 1000 * 1000 /* 1 MB */;

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
        }

        [TestMethod]
        public void TestCreate()
        {
            if (File.Exists(DefaultTestfilePath)) File.Delete(DefaultTestfilePath);

            var fileSystemOptions = new FileSystemOptions(DefaultTestfilePath, DefaultSize);

            Assert.IsFalse(File.Exists(DefaultTestfilePath));

            // Should call create, because the file does not exist
            var fileSystem = FileSystemFactory.CreateOrImport(fileSystemOptions);
            fileSystemOptions = fileSystem.FileSystemOptions;

            Assert.IsTrue(File.Exists(DefaultTestfilePath), String.Format("testfile {0} should exist!", DefaultTestfilePath));
            Assert.IsTrue(File.ReadAllText(DefaultTestfilePath).Length > 0);
            Assert.AreEqual(DefaultTestfilePath, fileSystemOptions.Location);
            Assert.AreEqual(DefaultSize, fileSystemOptions.DiskSize);

            FileSystemFactory.Delete(fileSystem);

        }

        [TestMethod]
        public void TestImport()
        {
            if (File.Exists(DefaultTestfilePath)) File.Delete(DefaultTestfilePath);

            Assert.IsTrue(File.Exists(ExampleTestfilePath), "Example test file must exist!");

            File.Copy(ExampleTestfilePath, DefaultTestfilePath);

            var fileSystemOptions = new FileSystemOptions(DefaultTestfilePath, DefaultSize);

            Assert.IsTrue(File.Exists(DefaultTestfilePath));

            // Should call import, because the file exists
            var fileSystem = FileSystemFactory.CreateOrImport(fileSystemOptions);
            fileSystemOptions = fileSystem.FileSystemOptions;

            Assert.IsTrue(File.Exists(DefaultTestfilePath), String.Format("testfile {0} should exist!", DefaultTestfilePath));
            Assert.IsTrue(File.ReadAllText(DefaultTestfilePath).Length > 0);
            Assert.AreEqual(DefaultTestfilePath, fileSystemOptions.Location);
            Assert.AreEqual(DefaultSize, fileSystemOptions.DiskSize);

            FileSystemFactory.Delete(fileSystem);
        }

        [TestMethod]
        public void TestDelete()
        {
            if (File.Exists(DefaultTestfilePath)) File.Delete(DefaultTestfilePath);

            var fileSystemOptions = new FileSystemOptions(DefaultTestfilePath, DefaultSize);

            Assert.IsFalse(File.Exists(DefaultTestfilePath));

            // Should call create, because the file does not exist
            var fileSystem = FileSystemFactory.CreateOrImport(fileSystemOptions);

            Assert.IsTrue(File.Exists(DefaultTestfilePath));

            FileSystemFactory.Delete(fileSystem);

            Assert.IsFalse(File.Exists(DefaultTestfilePath), "File should have been deleted!");
        }
    }
}
