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
using VFSBaseTests.Helpers;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemExtendedTest
    {
        private TestHelper _testHelper;
        private const string DefaultTestfileDirectoryPath = "../../../Testfiles/FileSystemExtendedTest/";

        [TestInitialize]
        public void InitTestHelper()
        {
            _testHelper = new TestHelper(DefaultTestfileDirectoryPath);
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            _testHelper.CleanupTestFolder();
        }

        [TestMethod]
        public void TestListObjects()
        {
            using (var m = _testHelper.GetManipulator())
            {
                var x = m.List("");
                Assert.IsNotNull(x);
            }
        }

        [TestMethod]
        public void TestReadBlock()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                var block = f.ReadBlock(0);
                Assert.AreEqual(0, block[0]);
            }
        }

        [TestMethod]
        public void TestIsSynchonizedDisk()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Assert.AreEqual(false, f.IsSynchronizedDisk);
            }
        }

        [TestMethod]
        public void TestMakeSynchonizedDisk()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                f.MakeSynchronizedDisk(77);
                Assert.IsTrue(f.IsSynchronizedDisk);
                Assert.AreEqual(77, f.FileSystemOptions.Id);
            }
        }

        [TestMethod]
        public void TestWriteBlock()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                var b = new byte[f.FileSystemOptions.BlockSize];
                b[99] = 7;
                f.WriteBlock(3, b);
                Assert.AreEqual(7, f.ReadBlock(3)[99]);
            }
        }

        [TestMethod]
        public void TestWriteConfig()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                f.WriteConfig(); // should not throw an exception
            }
        }

        [TestMethod]
        public void TestSearch()
        {
            using (var m = _testHelper.GetManipulator())
            {
                var l1 = m.Search("xxx", "/", true, false);
                Assert.AreEqual(0, l1.Count);
                m.CreateFolder("xxx");
                var l2 = m.Search("xxx", "/", true, false);
                Assert.AreEqual(1, l2.Count);
            }
        }
    }
}
