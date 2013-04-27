using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorFoldersTest
    {
        private const string DefaultTestfilePath = "../../../Testfiles/Testfile.vhs";
        private const string DummyImportFolderPath = "../../../Testfiles/DummyfolderImportFolders";
        private const string DummyExportFolderPath = "../../../Testfiles/DummyfolderExportFolders";
        private const long DefaultSize = 1024 * 1024 * 1024 /* 1 MB */;

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

        [TestInitialize]
        public void CreateDummyfiles()
        {
            if (Directory.Exists(DummyImportFolderPath)) Directory.Delete(DummyImportFolderPath, true);
            if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);
            Directory.CreateDirectory(DummyImportFolderPath);
            File.WriteAllText(Path.Combine(DummyImportFolderPath, "a"), "bli");
            File.WriteAllText(Path.Combine(DummyImportFolderPath, "b"), "bla");
            File.WriteAllText(Path.Combine(DummyImportFolderPath, "c"), "blub");
            Directory.CreateDirectory(Path.Combine(DummyImportFolderPath, "foo"));
            Directory.CreateDirectory(Path.Combine(DummyImportFolderPath, "bar"));
            File.WriteAllText(Path.Combine(DummyImportFolderPath, "foo", "d"), "ddd");
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            if (Directory.Exists(DummyImportFolderPath)) Directory.Delete(DummyImportFolderPath, true);
            if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);
            File.Delete(DefaultTestfilePath);
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

                m.CreateFolder("/test/foo/zzz");
                Assert.AreEqual(3, m.Folders("test/foo").Count);
                Assert.IsTrue(m.Folders("test/foo").Contains("zzz"));
            }
        }

        [TestMethod]
        public void TestCopyFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.CreateFolder("you");
                m.Copy("you", "me");
                Assert.IsTrue(m.Exists("me"));
                Assert.IsTrue(m.Exists("you"));

                m.CreateFolder("hello/world");
                m.Copy("hello/world", "hello/universe");
                Assert.IsTrue(m.Exists("hello/universe"));
                Assert.IsTrue(m.Exists("hello/world"));

                m.CreateFolder("foo/bar");
                m.Copy("foo/bar", "ta/da");
                Assert.IsTrue(m.Exists("ta/da"));
                Assert.IsTrue(m.Exists("foo/bar"));
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
            }
        }

        [TestMethod]
        public void TestCreateManyFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                // Could be set to more, but it is disabled, so the unit tests run fast
                for (var i = 0; i < 50; i++)
                {
                    m.CreateFolder("test" + i);
                    Assert.IsTrue(m.Exists("test" + i));
                }
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
        public void TestImportFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Assert.IsFalse(m.Exists("dummy"));
                Assert.IsFalse(m.Exists("dummy/a"));
                Assert.IsFalse(m.Exists("dummy/b"));
                Assert.IsFalse(m.Exists("dummy/c"));
                Assert.IsFalse(m.Exists("dummy/foo"));
                Assert.IsFalse(m.Exists("dummy/bar"));
                Assert.IsFalse(m.Exists("dummy/foo/d"));

                m.Import(DummyImportFolderPath, "dummy");
                
                Assert.IsTrue(m.Exists("dummy"));
                Assert.IsTrue(m.Exists("dummy/a"));
                Assert.IsTrue(m.Exists("dummy/b"));
                Assert.IsTrue(m.Exists("dummy/c"));
                Assert.IsTrue(m.Exists("dummy/foo"));
                Assert.IsTrue(m.Exists("dummy/bar"));
                Assert.IsTrue(m.Exists("dummy/foo/d"));

                Assert.IsFalse(m.Exists("dummy/d"));
            }
        }

        [TestMethod]
        public void TestExportFolder()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);

                m.Import(DummyImportFolderPath, "dummy");

                if(!m.Exists("dummy")) Assert.Inconclusive();

                Assert.IsFalse(Directory.Exists(DummyExportFolderPath));

                m.Export("dummy", DummyExportFolderPath);

                Assert.IsTrue(Directory.Exists(DummyExportFolderPath));
                Assert.IsTrue(File.Exists(Path.Combine(DummyExportFolderPath, "a")));
                Assert.IsTrue(File.Exists(Path.Combine(DummyExportFolderPath, "b")));
                Assert.IsTrue(File.Exists(Path.Combine(DummyExportFolderPath, "c")));
                Assert.IsTrue(Directory.Exists(Path.Combine(DummyExportFolderPath, "foo")));
                Assert.IsTrue(Directory.Exists(Path.Combine(DummyExportFolderPath, "bar")));
                Assert.IsTrue(File.Exists(Path.Combine(DummyExportFolderPath, "foo", "d")));

                Assert.IsFalse(File.Exists(Path.Combine(DummyExportFolderPath, "d")));

                Assert.AreEqual("bli", File.ReadAllText(Path.Combine(DummyExportFolderPath, "a")));
                Assert.AreEqual("bla", File.ReadAllText(Path.Combine(DummyExportFolderPath, "b")));
                Assert.AreEqual("blub", File.ReadAllText(Path.Combine(DummyExportFolderPath, "c")));
                Assert.AreEqual("ddd", File.ReadAllText(Path.Combine(DummyExportFolderPath, "foo", "d")));
            }
        }
    }
}
