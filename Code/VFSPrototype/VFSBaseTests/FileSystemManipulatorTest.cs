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
            Assert.IsTrue(m.Exists("test"));

            m.CreateFolder("test/foo/bar/tamtam");
            Assert.IsTrue(m.Exists("test/foo/bar/tamtam"));
            Assert.IsTrue(m.Exists("test/foo"));
            Assert.IsTrue(m.Exists("test/foo/bar"));
            Assert.IsFalse(m.Exists("Test/foo/bar/tamTam"));

            m.CreateFolder("some/test/test/test/here");
            Assert.IsTrue(m.Exists("some/test/test/test/here"));
            Assert.IsTrue(m.Exists("some/test/test"));

            m.CreateFolder("test/xxx");
            Assert.IsTrue(m.Exists("test/xxx"));
        }


        [TestMethod]
        public void TestDeleteFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.CreateFolder("test");
            m.Delete("test");
            Assert.IsFalse(m.Exists("test"));

            m.CreateFolder("test/foo/bar/tamtam");
            m.Delete("test/foo");
            Assert.IsFalse(m.Exists("test/foo"));
            Assert.IsFalse(m.Exists("test/foo/bar"));
            Assert.IsFalse(m.Exists("test/foo/bar/tamtam"));
            Assert.IsTrue(m.Exists("test"));

            m.CreateFolder("some/test/test/test/here");
            m.Delete("some/test/test/test/here");
            Assert.IsFalse(m.Exists("some/test/test/test/here"));
            Assert.IsTrue(m.Exists("some/test/test/test"));

            m.CreateFolder("test/xxx");
            m.Delete("test");
            Assert.IsFalse(m.Exists("test"));
            Assert.IsFalse(m.Exists("test/xxx"));
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void TestInvalidDeleteFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            Assert.IsFalse(m.Exists("test"));
            m.Delete("test");
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
            Assert.IsTrue(m.Exists("test.txt"));

            m.CreateFolder("folder");
            m.ImportFile(testFileSource,   "folder/test.txt");
            Assert.IsTrue(m.Exists("folder/test.txt"));

            m.ImportFile(testFileSource,   "this/is/a/test/hello.txt");
            Assert.IsTrue(m.Exists("this/is/a/test/hello.txt"));
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
            Assert.IsTrue(m.Exists("test.txt"));
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
            m.Delete("test.txt");
            Assert.IsFalse(m.Exists("test.txt"));

            m.ImportFile(testFileSource, "hello/test.txt");
            m.Delete("hello/test.txt");
            Assert.IsFalse(m.Exists("hello/test.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void TestInvalidDeleteFile()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.Delete("test.txt");
        }

        [TestMethod]
        public void TestMoveFolder()
        {
            var fs = InitTestFileSystem(DefaultTestfilePath, DefaultSize);
            var m = InitTestFileSystemManipulator(fs);

            m.CreateFolder("hello/world");
           // m.MoveFolder("hello/world", "hello/universe");
            Assert.IsTrue(m.Exists("hello/universe"));
            Assert.IsFalse(m.Exists("hello/world"));


            m.CreateFolder("foo/bar");
           // m.MoveFolder("foo/bar", "ta/da");
            Assert.IsTrue(m.Exists("ta/da"));
            Assert.IsFalse(m.Exists("foo/bar"));
        }


        [TestMethod]
        public void TestMoveFile()
        {
            
        }
    }
}
