using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorTest
    {
        const string DefaultTestfilePath = "../../../Testfiles/Testfile.vhs";
        private const long DefaultSize = 1000 * 1000 * 1000 /* 1 MB */;

        private static FileSystemOptions InitTestFileSystemData(string testfilePath, long size)
        {
            if (File.Exists(testfilePath)) File.Delete(testfilePath);
            var fileSystemData = new FileSystemOptions(testfilePath, size);
            Assert.IsFalse(File.Exists(testfilePath), String.Format("testfile {0} should not exist!", testfilePath));
            return fileSystemData;
        }

        private static IFileSystemTextManipulator InitTestFileSystemManipulator()
        {
            return new FileSystemTextManipulator(InitTestFileSystemData(DefaultTestfilePath, DefaultSize));
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
        }

        [TestMethod]
        public void TestCreateFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
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

                // TODO: fix this?
                //m.CreateFolder("/test/foo/zzz");
                //Assert.AreEqual(4, m.Folders("test/foo").Count);
                //Assert.IsTrue(m.Folders("test/foo").Contains("zzz"));
            }
        }

        [TestMethod]
        public void TestDeleteFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Assert.IsFalse(m.Exists("test"));
                m.CreateFolder("test");
                Assert.IsTrue(m.Exists("test"));
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
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void TestInvalidDeleteFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Assert.IsFalse(m.Exists("test"));
                m.Delete("test");
            }
        }


        [TestMethod]
        public void TestImportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.ImportFile(testFileSource, "test.txt");
                Assert.IsTrue(m.Exists("test.txt"));

                m.CreateFolder("folder");
                m.ImportFile(testFileSource, "folder/test.txt");
                Assert.IsTrue(m.Exists("folder/test.txt"));

                m.ImportFile(testFileSource, "this/is/a/test/hello.txt");
                Assert.IsTrue(m.Exists("this/is/a/test/hello.txt"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestInvalidImportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);

                m.ImportFile(testFileSource, "test.txt");
            }
        }

        [TestMethod]
        public void TestExportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.ImportFile(testFileSource, "test.txt");
                m.ExportFile("test.txt", "export.txt");
                Assert.IsTrue(m.Exists("test.txt"));
            }
            Assert.IsTrue(File.Exists("export.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestInvalidExportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.ExportFile("test.txt", "export.txt");
            }
        }

        [TestMethod]
        public void TestDeleteFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.ImportFile(testFileSource, "test.txt");
                m.Delete("test.txt");
                Assert.IsFalse(m.Exists("test.txt"));

                m.ImportFile(testFileSource, "hello/test.txt");
                m.Delete("hello/test.txt");
                Assert.IsFalse(m.Exists("hello/test.txt"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void TestInvalidDeleteFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.Delete("test.txt");
            }
        }

        [TestMethod]
        public void TestMoveFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.CreateFolder("you");
                m.Move("you", "me");
                Assert.IsTrue(m.Exists("me"));
                Assert.IsFalse(m.Exists("you"));

                m.CreateFolder("hello/world");
                m.Move("hello/world", "hello/universe");
                Assert.IsTrue(m.Exists("hello/universe"));
                Assert.IsFalse(m.Exists("hello/world"));


                m.CreateFolder("foo/bar");
                m.Move("foo/bar", "ta/da");
                Assert.IsTrue(m.Exists("ta/da"));
                Assert.IsFalse(m.Exists("foo/bar"));
            }
        }


        [TestMethod]
        public void TestMoveFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.ImportFile(testFileSource, "you.txt");
                m.Move("you.txt", "me.txt");
                Assert.IsTrue(m.Exists("me.txt"));
                Assert.IsFalse(m.Exists("you.txt"));

                m.ImportFile(testFileSource, "hello/world.txt");
                m.Move("hello/world.txt", "hello/universe.txt");
                Assert.IsTrue(m.Exists("hello/universe.txt"));
                Assert.IsFalse(m.Exists("hello/world.txt"));

                m.ImportFile(testFileSource, "foo/bar.txt");
                m.Move("foo/bar.txt", "ta/da.txt");
                Assert.IsTrue(m.Exists("ta/da.txt"));
                Assert.IsFalse(m.Exists("foo/bar.txt"));
            }
        }

        [TestMethod]
        public void TestListRootFolders()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Assert.AreEqual(0, m.Folders("").Count);
                Assert.AreEqual(0, m.Folders("/").Count);

                m.CreateFolder("test");

                Assert.AreEqual(1, m.Folders("").Count);
                Assert.AreEqual(1, m.Folders("/").Count);
                Assert.IsTrue(m.Folders("").Contains("test"));
                Assert.IsTrue(m.Folders("/").Contains("test"));

                m.CreateFolder("test");

                Assert.AreEqual(1, m.Folders("").Count);
                Assert.AreEqual(1, m.Folders("/").Count);

                m.CreateFolder("foo");

                Assert.AreEqual(2, m.Folders("").Count);
                Assert.AreEqual(2, m.Folders("/").Count);

                m.CreateFolder("bar");

                Assert.AreEqual(3, m.Folders("").Count);
                Assert.AreEqual(3, m.Folders("/").Count);
            }
        }

        [TestMethod]
        public void TestListDirectoryFolders()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.CreateFolder("test");

                Assert.AreEqual(0, m.Folders("test").Count);
                Assert.AreEqual(0, m.Folders("/test").Count);

                m.CreateFolder("test/foo");
                Assert.AreEqual(1, m.Folders("test").Count);
                Assert.IsTrue(m.Folders("test").Contains("foo"));

                m.CreateFolder("test/foo");
                Assert.AreEqual(1, m.Folders("test").Count);
                m.CreateFolder("test/bar");
                Assert.AreEqual(2, m.Folders("test").Count);
                m.CreateFolder("test/xxx");
                Assert.AreEqual(3, m.Folders("test").Count);
                m.Delete("test/xxx");
                Assert.AreEqual(2, m.Folders("test").Count);

                Assert.AreEqual(0, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/bar");
                Assert.AreEqual(1, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/bar");
                Assert.AreEqual(1, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/foobar");
                Assert.AreEqual(2, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/xxx");
                Assert.AreEqual(3, m.Folders("test/foo").Count);

                Assert.IsTrue(m.Folders("test/foo").Contains("bar"));
                Assert.IsTrue(m.Folders("test/foo").Contains("foobar"));
                Assert.IsTrue(m.Folders("test/foo").Contains("xxx"));

                m.Delete("test/foo/xxx");
                Assert.AreEqual(2, m.Folders("test/foo").Count);

                Assert.IsTrue(m.Folders("test/foo").Contains("bar"));
                Assert.IsTrue(m.Folders("test/foo").Contains("foobar"));
                Assert.IsFalse(m.Folders("test/foo").Contains("xxx"));
            }
        }

        [TestMethod]
        public void TestListSubdirectoryFolders()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.CreateFolder("test/foo");
                Assert.AreEqual(0, m.Folders("test/foo").Count);
                Assert.AreEqual(0, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/bar");
                Assert.AreEqual(1, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/bar");
                Assert.AreEqual(1, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/foobar");
                Assert.AreEqual(2, m.Folders("test/foo").Count);
                m.CreateFolder("test/foo/xxx");
                Assert.AreEqual(3, m.Folders("test/foo").Count);
                m.Delete("test/foo/xxx");
                Assert.AreEqual(2, m.Folders("test/foo").Count);
            }
        }

        [TestMethod]
        public void TestExists()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Assert.IsTrue(m.Exists(""));
                Assert.IsTrue(m.Exists("/"));

                Assert.IsFalse(m.Exists("test"));
                Assert.IsFalse(m.Exists("/test"));

                m.CreateFolder("test");

                Assert.IsTrue(m.Exists("test"));
                Assert.IsTrue(m.Exists("/test"));
            }
        }

        [TestMethod]
        public void TestIsDirectorySimple()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Assert.IsTrue(m.IsDirectory(""));
                Assert.IsTrue(m.IsDirectory("/"));
                Assert.IsFalse(m.IsDirectory("test"));
                m.CreateFolder("test");
                Assert.IsTrue(m.IsDirectory("test"));
                Assert.IsTrue(m.IsDirectory("/test"));

                Assert.IsFalse(m.IsDirectory("test/foo"));
                m.CreateFolder("test/foo");
                Assert.IsTrue(m.IsDirectory("test/foo"));
                Assert.IsTrue(m.IsDirectory("/test/foo"));
            }
        }

        [TestMethod]
        public void TestLsDirectoryWithFiles()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                Assert.IsFalse(m.Exists("test"));
                m.ImportFile(testFileSource, "test");
                Assert.IsTrue(m.Exists("test"));

                Assert.IsFalse(m.IsDirectory("test"));
                Assert.IsFalse(m.IsDirectory("/test"));
            }
        }
    }
}
