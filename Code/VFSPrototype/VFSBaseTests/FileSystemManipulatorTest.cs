using System;
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

        private static FileSystem InitTestFileSystem(string testfilePath, ulong size)
        {
            if (File.Exists(testfilePath)) File.Delete(testfilePath);
            var fileSystem = new FileSystem(new FileSystemOptions(testfilePath, size));
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

            // Create test file
            const string testFileSource = "test.txt";
            if (File.Exists(testFileSource)) File.Delete(testFileSource);
            var file = File.Create(testFileSource);
            file.Close();

            
            m.ImportFile(testFileSource,   "test.txt");
            Assert.IsTrue(m.DoesFileExists("test.txt"));

            m.CreateFolder("folder");
            m.ImportFile(testFileSource,   "folder/test.txt");
            Assert.IsTrue(m.DoesFileExists("folder/test.txt"));

            m.ImportFile(testFileSource,   "this/is/a/test/hello.txt");
            Assert.IsTrue(m.DoesFileExists("this/is/a/test/hello.txt"));
        }
        
        [TestMethod]
        [ExpectedException(typeof (FileNotFoundException))]
        public void TestInvalidImportFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            // Delete test file
            const string testFileSource = "test.txt";
            if (File.Exists(testFileSource)) File.Delete(testFileSource);

            m.ImportFile(testFileSource, "test.txt");
        }

        [TestMethod]
        public void TestExportFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            // Create test file
            const string testFileSource = "test.txt";
            if (File.Exists(testFileSource)) File.Delete(testFileSource);
            var file = File.Create(testFileSource);
            file.Close();

            m.ImportFile(testFileSource, "test.txt");
            m.ExportFile("test.txt", "export.txt");
            Assert.IsTrue(m.DoesFileExists("test.txt"));
            Assert.IsTrue(File.Exists("export.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof (FileNotFoundException))]
        public void TestInvalidExportFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.ExportFile("test.txt", "export.txt");
        }

        [TestMethod]
        public void TestDeleteFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            // Create test file
            const string testFileSource = "test.txt";
            if (File.Exists(testFileSource)) File.Delete(testFileSource);
            var file = File.Create(testFileSource);
            file.Close();

            m.ImportFile(testFileSource, "test.txt");
            m.DeleteFile("test.txt");
            Assert.IsFalse(m.DoesFileExists("test.txt"));

            m.ImportFile(testFileSource, "hello/test.txt");
            m.DeleteFile("hello/test.txt");
            Assert.IsFalse(m.DoesFileExists("hello/test.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestInvalidDeleteFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.DeleteFile("test.txt");
        }

        [TestMethod]
        public void TestMoveFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.CreateFolder("hello/world");
           // m.MoveFolder("hello/world", "hello/universe");
            Assert.IsTrue(m.DoesFolderExist("hello/universe"));
            Assert.IsFalse(m.DoesFileExists("hello/world"));


            m.CreateFolder("foo/bar");
           // m.MoveFolder("foo/bar", "ta/da");
            Assert.IsTrue(m.DoesFolderExist("ta/da"));
            Assert.IsFalse(m.DoesFileExists("foo/bar"));
        }


        [TestMethod]
        public void TestMoveFile()
        {
            
        }
    }
}
