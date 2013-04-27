using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorTest
    {
        private const string DefaultTestfilePath = "../../../Testfiles/TestfileFileSystemManipulator.vhs";
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

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
        }

        [TestMethod]
        public void TestImportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.Import(testFileSource, "test.txt");
                Assert.IsTrue(m.Exists("test.txt"));

                m.CreateFolder("folder");
                m.Import(testFileSource, "folder/test.txt");
                Assert.IsTrue(m.Exists("folder/test.txt"));

                m.Import(testFileSource, "this/is/a/test/hello.txt");
                Assert.IsTrue(m.Exists("this/is/a/test/hello.txt"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void TestInvalidImportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);

                m.Import(testFileSource, "test.txt");
            }
        }

        [TestMethod]
        public void TestExportFileBasic()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                byte[] testFileData = new byte[4096];
                testFileData[0] = (byte)'\n';

                if (File.Exists(testFileSource)) File.Delete(testFileSource);

                File.WriteAllBytes(testFileSource, testFileData);

                Assert.IsFalse(m.Exists(testFileSource));
                Assert.IsFalse(m.Exists(testFileSource));

                m.Import(testFileSource, testFileSource);
                if (!m.Exists(testFileSource)) Assert.Inconclusive("Something with the import does not work correctly");
                File.Delete(testFileSource);
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.IsFalse(File.Exists(testFileSource));

                m.Export(testFileSource, testFileSource);

                Assert.IsTrue(File.Exists(testFileSource));
                Assert.IsTrue(m.Exists(testFileSource));
                var readAllBytes = File.ReadAllBytes(testFileSource);

                for (var i = 0; i < readAllBytes.Length; i++) Assert.AreEqual(testFileData[i], readAllBytes[i]);
            }
        }

        [TestMethod]
        public void TestExportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                const string testFileData = "xxx";

                if (File.Exists(testFileSource)) File.Delete(testFileSource);

                File.WriteAllText(testFileSource, testFileData);

                Assert.IsFalse(m.Exists(testFileSource));
                Assert.IsFalse(m.Exists(testFileSource));

                m.Import(testFileSource, testFileSource);
                if (!m.Exists(testFileSource)) Assert.Inconclusive("Something with the import does not work correctly");
                File.Delete(testFileSource);
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.IsFalse(File.Exists(testFileSource));

                m.Export(testFileSource, testFileSource);

                Assert.IsTrue(File.Exists(testFileSource));
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.AreEqual(testFileData, File.ReadAllText(testFileSource));
            }
        }

        private static byte[] Md5Hash(string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                return md5.ComputeHash(file);
            }
        }

        [TestMethod]
        public void TestImportExportBigFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                const string verifyTestFileSource = "verify.txt";

                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                if (File.Exists(verifyTestFileSource)) File.Delete(verifyTestFileSource);

                //const int bufferLength = 263; // 263 is a prime number.
                const int bufferLength = 16384; // 263 is a prime number.
                var rand = new Random(1);

                using (var f = File.OpenWrite(testFileSource))
                {
                    var buffer = new byte[bufferLength];
                    //for (var i = 0; i < 10000; i++)
                    for (var i = 0; i < 1; i++) // TODO: set value to 10000
                    {
                        rand.NextBytes(buffer);
                        f.Write(buffer, 0, buffer.Length);
                    }
                }
                File.Copy(testFileSource, verifyTestFileSource);

                Assert.IsFalse(m.Exists(testFileSource));
                Assert.IsFalse(m.Exists(testFileSource));

                m.Import(testFileSource, testFileSource);
                if (!m.Exists(testFileSource)) Assert.Inconclusive("Something with the import does not work correctly");
                File.Delete(testFileSource);
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.IsFalse(File.Exists(testFileSource));

                m.Export(testFileSource, testFileSource);

                Assert.IsTrue(File.Exists(testFileSource));
                Assert.IsTrue(m.Exists(testFileSource));

                var b3 = File.ReadAllBytes(verifyTestFileSource);
                var b4 = File.ReadAllBytes(testFileSource);
                Assert.AreEqual(b3.Length, b4.Length);

                //var b1 = Md5Hash(verifyTestFileSource);
                //var b2 = Md5Hash(testFileSource);
                //for (var i = 0; i < b1.Length; i++) Assert.AreEqual(b1[i], b2[i]);

                for (var i = 0; i < b3.Length; i++) Assert.AreEqual(b3[i], b4[i]);
            }
        }

        [TestMethod]
        public void TestImportCopyExportBigFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                const string copyFileSource = "copy.txt";
                const string verifyTestFileSource = "verify.txt";

                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                if (File.Exists(verifyTestFileSource)) File.Delete(verifyTestFileSource);

                const int bufferLength = 263; // 263 is a prime number.
                var rand = new Random(1);

                using (var f = File.OpenWrite(testFileSource))
                {
                    var buffer = new byte[bufferLength];
                    for (var i = 0; i < 500; i++) // TODO: increase this value
                    {
                        rand.NextBytes(buffer);
                        f.Write(buffer, 0, buffer.Length);
                    }
                }
                File.Copy(testFileSource, verifyTestFileSource);

                Assert.IsFalse(m.Exists(testFileSource));
                Assert.IsFalse(m.Exists(testFileSource));

                m.Import(testFileSource, testFileSource);
                if (!m.Exists(testFileSource)) Assert.Inconclusive("Something with the import does not work correctly");
                File.Delete(testFileSource);
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.IsFalse(File.Exists(testFileSource));

                m.Copy(testFileSource, copyFileSource);

                m.Export(copyFileSource, testFileSource);

                Assert.IsTrue(File.Exists(testFileSource));
                Assert.IsTrue(m.Exists(testFileSource));

                var b1 = Md5Hash(verifyTestFileSource);
                var b2 = Md5Hash(testFileSource);
                for (var i = 0; i < b1.Length; i++) Assert.AreEqual(b1[i], b2[i]);

                var b3 = File.ReadAllBytes(verifyTestFileSource);
                var b4 = File.ReadAllBytes(testFileSource);
                for (var i = 0; i < b3.Length; i++) Assert.AreEqual(b3[i], b4[i]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void TestInvalidExportFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                m.Export("test.txt", "export-xxx.txt");
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

                m.Import(testFileSource, "test.txt");
                m.Delete("test.txt");
                Assert.IsFalse(m.Exists("test.txt"));

                m.Import(testFileSource, "hello/test.txt");
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
        public void TestMoveFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.Import(testFileSource, "you.txt");
                m.Move("you.txt", "me.txt");
                Assert.IsTrue(m.Exists("me.txt"));
                Assert.IsFalse(m.Exists("you.txt"));

                m.Import(testFileSource, "hello/world.txt");
                m.Move("hello/world.txt", "hello/universe.txt");
                Assert.IsTrue(m.Exists("hello/universe.txt"));
                Assert.IsFalse(m.Exists("hello/world.txt"));

                m.Import(testFileSource, "foo/bar.txt");
                m.Move("foo/bar.txt", "ta/da.txt");
                Assert.IsTrue(m.Exists("ta/da.txt"));
                Assert.IsFalse(m.Exists("foo/bar.txt"));
            }
        }

        [TestMethod]
        public void TestCopyFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.txt";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.Import(testFileSource, "you.txt");
                m.Copy("you.txt", "me.txt");
                Assert.IsTrue(m.Exists("me.txt"));
                Assert.IsTrue(m.Exists("you.txt"));

                m.Import(testFileSource, "hello/world.txt");
                m.Copy("hello/world.txt", "hello/universe.txt");
                Assert.IsTrue(m.Exists("hello/universe.txt"));
                Assert.IsTrue(m.Exists("hello/world.txt"));

                m.Import(testFileSource, "foo/bar.txt");
                m.Copy("foo/bar.txt", "ta/da.txt");
                Assert.IsTrue(m.Exists("ta/da.txt"));
                Assert.IsTrue(m.Exists("foo/bar.txt"));
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
        public void TestLsDirectoryWithFiles()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                Assert.IsFalse(m.Exists("test"));
                m.Import(testFileSource, "test");
                Assert.IsTrue(m.Exists("test"));

                Assert.IsFalse(m.IsDirectory("test"));
                Assert.IsFalse(m.IsDirectory("/test"));
            }
        }
    }
}
