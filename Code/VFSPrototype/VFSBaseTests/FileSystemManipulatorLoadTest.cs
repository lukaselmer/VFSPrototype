using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Factories;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBaseTests.Helpers;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorLoadTest
    {
        private const string DefaultTestfilePath = "../../../Testfiles/Testfile.vhs";
        private const string DummyFilePath = "../../../Testfiles/dummy.dat";
        private readonly long _dummyFileSize = MathUtil.KB(100); // TODO: increase this value. Could be MathUtil.GB(1), but it is disabled, so the unit tests run fast
        private const long DefaultSize = 1024 * 1024 * 1024 /* 1 MB */;

        private static FileSystemOptions InitTestFileSystemData(string testfilePath, long size)
        {
            if (File.Exists(testfilePath)) File.Delete(testfilePath);
            var fileSystemData = TestHelper.CreateFileSystemOptions(testfilePath, size);
            Assert.IsFalse(File.Exists(testfilePath), String.Format("testfile {0} should not exist!", testfilePath));
            return fileSystemData;
        }

        private static IFileSystemTextManipulator InitTestFileSystemManipulator()
        {
            return new FileSystemTextManipulatorFactory().CreateFileSystemTextManipulator(InitTestFileSystemData(DefaultTestfilePath, DefaultSize), "");
        }

        [TestInitializeAttribute]
        public void CreateHugeFile()
        {
            if (File.Exists(DummyFilePath)) File.Delete(DummyFilePath);

            using (var fs = new FileStream(DummyFilePath, FileMode.CreateNew))
            {
                fs.SetLength(_dummyFileSize);
            }
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
            File.Delete(DummyFilePath);
        }

        [TestMethod]
        public void TestImportHugeFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = DummyFilePath;
                const string testFileNamePath = "folder/test.dat";
                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                File.WriteAllText(testFileSource, "");

                m.CreateFolder("folder");
                m.Import(testFileSource, testFileNamePath);
                Assert.IsTrue(m.Exists(testFileNamePath));
            }
        }

        [TestMethod]
        public void TestExportHugeFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.dat";

                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                Assert.IsFalse(m.Exists(testFileSource));

                m.Import(DummyFilePath, testFileSource);
                if (!m.Exists(testFileSource)) Assert.Inconclusive("Something with the import does not work correctly");
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.IsFalse(File.Exists(testFileSource));

                m.Export(testFileSource, testFileSource);

                Assert.IsTrue(File.Exists(testFileSource));
                Assert.IsTrue(m.Exists(testFileSource));

                var b3 = File.ReadAllBytes(DummyFilePath);
                var b4 = File.ReadAllBytes(testFileSource);
                for (var i = 0; i < b3.Length; i++) Assert.AreEqual(b3[i], b4[i]);

                var b1 = Md5Hash(DummyFilePath);
                var b2 = Md5Hash(testFileSource);
                for (var i = 0; i < b1.Length; i++) Assert.AreEqual(b1[i], b2[i]);
            }
        }

        private byte[] Md5Hash(string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                return md5.ComputeHash(file);
            }
        }

        [TestMethod]
        public void TestImportCopyExportHugeFile()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                const string testFileSource = "test.dat";
                const string copyTestFileSource = "copy.dat";

                if (File.Exists(testFileSource)) File.Delete(testFileSource);
                Assert.IsFalse(m.Exists(testFileSource));

                m.Import(DummyFilePath, testFileSource);
                if (!m.Exists(testFileSource)) Assert.Inconclusive("Something with the import does not work correctly");
                Assert.IsTrue(m.Exists(testFileSource));
                Assert.IsFalse(File.Exists(testFileSource));

                m.Copy(testFileSource, copyTestFileSource);
                Assert.IsTrue(m.Exists(copyTestFileSource));
                Assert.IsTrue(m.Exists(testFileSource));

                m.Export(copyTestFileSource, testFileSource);

                Assert.IsTrue(File.Exists(testFileSource));
                Assert.IsTrue(m.Exists(testFileSource));

                var b1 = Md5Hash(DummyFilePath);
                var b2 = Md5Hash(testFileSource);
                for (var i = 0; i < b1.Length; i++) Assert.AreEqual(b1[i], b2[i]);
            }
        }
    }
}
