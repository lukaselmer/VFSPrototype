﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorTest
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

        private static FileSystemManipulator InitTestFileSystemManipulator(FileSystem fileSystem)
        {
            return new FileSystemManipulator(fileSystem);
        }

        [TestMethod]
        public void TestCreateFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.CreateFolder("test");
            Assert.IsTrue(m.DoesFolderExist("test"));

            m.CreateFolder("test/foo/bar/tamtam");
            Assert.IsTrue(m.DoesFolderExist("test/foo/bar/tamtam"));
            Assert.IsTrue(m.DoesFolderExist("test/foo"));
            Assert.IsTrue(m.DoesFolderExist("test/foo/bar"));
            Assert.IsFalse(m.DoesFolderExist("Test/foo/bar/tamTam"));

            m.CreateFolder("some/test/test/test/here");
            Assert.IsTrue(m.DoesFolderExist("some/test/test/test/here"));
            Assert.IsTrue(m.DoesFolderExist("some/test/test"));

            m.CreateFolder("test/xxx");
            Assert.IsTrue(m.DoesFolderExist("test/xxx"));
        }


        [TestMethod]
        public void TestDeleteFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.CreateFolder("test");
            m.DeleteFolder("test");
            Assert.IsFalse(m.DoesFolderExist("test"));

            m.CreateFolder("test/foo/bar/tamtam");
            m.DeleteFolder("test/foo");
            Assert.IsFalse(m.DoesFolderExist("test/foo"));
            Assert.IsFalse(m.DoesFolderExist("test/foo/bar"));
            Assert.IsFalse(m.DoesFolderExist("test/foo/bar/tamtam"));
            Assert.IsTrue(m.DoesFolderExist("test"));

            m.CreateFolder("some/test/test/test/here");
            m.DeleteFolder("some/test/test/test/here");
            Assert.IsFalse(m.DoesFolderExist("some/test/test/test/here"));
            Assert.IsTrue(m.DoesFolderExist("some/test/test/test"));

            m.CreateFolder("test/xxx");
            m.DeleteFolder("test");
            Assert.IsFalse(m.DoesFolderExist("test"));
            Assert.IsFalse(m.DoesFolderExist("test/xxx"));
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void TestInvalidDeleteFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            Assert.IsFalse(m.DoesFolderExist("test"));
            m.DeleteFolder("test");
        }


        [TestMethod]
        public void TestImportFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            const string testFileSource = "test.txt";
            const string testFileDest = "test.txt";
            File.Create(testFileSource);
            
            m.ImportFile(testFileSource, testFileDest);
            
            Assert.IsTrue(m.FileExists(testFileDest));

            File.Delete(testFileSource);
        }

        [TestMethod]
        public void TestExportFile()
        {
            
        }

        [TestMethod]
        public void TestDeleteFile()
        {
            
        }
    }
}
