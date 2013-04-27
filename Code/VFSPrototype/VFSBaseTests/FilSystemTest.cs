﻿using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence;

namespace VFSBaseTests
{
    [TestClass]
    public class FilSystemTest
    {
        private const string DefaultTestfilePath = "../../../Testfiles/testfile.vhs";
        private readonly long _defaultSize = MathUtil.MB(5);

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
        }


        private IFileSystem GetFileSystem(string path = DefaultTestfilePath)
        {
            var o = new FileSystemOptions(path, _defaultSize);
            return FileSystemFactory.CreateOrImport(o);
        }

        [TestMethod]
        public void TestCreateTopLevelFolder()
        {
            using (var fs = GetFileSystem())
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, "test");
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }

            using (var fs = GetFileSystem())
            {
                //TODO: make this true! Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }
        }

        [TestMethod]
        public void TestNamesShouldSupportUtf8()
        {
            var name = "∀α,β∈∑α≤β∧β≥α=>α=β";
            using (var fs = GetFileSystem())
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, name);
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }

            using (var fs = GetFileSystem())
            {
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
                Assert.IsTrue(fs.Folders(fs.Root).First().Name == name);
            }
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestRootFolderSetNotPossible()
        {
            new RootFolder().BlockNumber = 42;
        }
    }
}
