﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorCallbacksTest
    {
        private const string DefaultTestfilePath = "../../../Testfiles/Testfile.vhs";
        private const string DummyImportFolderPath = "../../../Testfiles/DummyfolderImport";
        private const string DummyExportFolderPath = "../../../Testfiles/DummyfolderExport";
        private const long DefaultSize = 1024 * 1024 * 1024 /* 1 MB */;

        private static FileSystemOptions InitTestFileSystemData(string testfilePath, long size)
        {
            if (File.Exists(testfilePath)) File.Delete(testfilePath);
            var fileSystemData = new FileSystemOptions(testfilePath, size);
            Assert.IsFalse(File.Exists(testfilePath), String.Format("testfile {0} actual not exist!", testfilePath));
            return fileSystemData;
        }

        private static IFileSystemTextManipulator InitTestFileSystemManipulator()
        {
            return new FileSystemTextManipulator(InitTestFileSystemData(DefaultTestfilePath, DefaultSize));
        }

        [TestInitialize]
        public void CreateDummyfiles()
        {
            Directory.Delete(DummyImportFolderPath, true);
            Directory.Delete(DummyExportFolderPath, true);
            Directory.CreateDirectory(DummyImportFolderPath);
            File.WriteAllText(DummyImportFolderPath + "/a", "bli");
            File.WriteAllText(DummyImportFolderPath + "/b", "bla");
            File.WriteAllText(DummyImportFolderPath + "/c", "blub");
        }

        [TestCleanup]
        public void RemoveDummyfiles()
        {
            Directory.Delete(DummyImportFolderPath, true);
            Directory.Delete(DummyExportFolderPath, true);
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
        }

        [TestMethod]
        public void TestImportAbort()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                var completed = false;
                var success = false;

                var totalCounter = new CountTester(3);

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks(() => true, b => { completed = true; success = b; }));
                Assert.IsFalse(m.Exists("dummy"));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, false);
            }
        }

        [TestMethod]
        public void TestImportCallbacks()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                var completed = false;
                var success = false;

                var totalCounter = new CountTester(3);
                Action<int> testTotalToProcess = totalCounter.Up;
                Action<int> testCurrentlyProcessed = new CountTester(3, totalCounter).Up;

                m.Import(DummyExportFolderPath, "dummy", new ImportCallbacks(() => false, b => { completed = true; success = b; }, testTotalToProcess, testCurrentlyProcessed));
                Assert.IsTrue(m.Exists("dummy"));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, true);
            }
        }

        [TestMethod]
        public void TestExportAbort()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Directory.Delete(DummyExportFolderPath, true);

                var completed = false;
                var success = false;

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks());
                m.Export("dummy", DummyExportFolderPath, new ExportCallbacks(() => true, b => { completed = true; success = b; }));
                Assert.IsFalse(Directory.Exists(DummyExportFolderPath));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, false);
            }
        }

        [TestMethod]
        public void TestExportCallbacks()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Directory.Delete(DummyExportFolderPath, true);

                var completed = false;
                var success = false;

                var totalCounter = new CountTester(3);
                Action<int> testTotalToProcess = totalCounter.Up;
                Action<int> testCurrentlyProcessed = new CountTester(3, totalCounter).Up;

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks(() => false, b => { completed = true; success = b; }, testTotalToProcess, testCurrentlyProcessed));
                Assert.IsTrue(Directory.Exists(DummyExportFolderPath));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, true);
            }
        }

        [TestMethod]
        public void TestCopyAbort()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Directory.Delete(DummyExportFolderPath, true);

                var completed = false;
                var success = false;

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks());
                m.Copy("dummy", "dummy2", new CopyCallbacks(() => true, b => { completed = true; success = b; }));
                Assert.IsFalse(m.Exists("dummy"));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, false);
            }
        }

        [TestMethod]
        public void TestCopyCallbacks()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                Directory.Delete(DummyExportFolderPath, true);

                var completed = false;
                var success = false;

                m.Copy("dummy", "dummy2", new CopyCallbacks(() => false, b => { completed = true; success = b; }));
                Assert.IsTrue(m.Exists("dummy"));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, true);
            }
        }

        public class CountTester
        {
            private readonly int _max;
            private readonly CountTester _total;
            private int _frozenTotal;
            private bool _totalFrozen;

            public CountTester(int max, CountTester total = null)
            {
                _max = max;
                _total = total;
                Expected = 1;
            }

            public int Expected { get; private set; }

            public void Up(int actual)
            {
                if (_total != null)
                {
                    FreezeTotal();
                    Assert.AreEqual(_frozenTotal, _total.Expected);
                }

                if (actual > _max) Assert.Fail("Too many calls");
                Assert.AreEqual(Expected, actual);
                Expected += 1;
            }

            private void FreezeTotal()
            {
                if (_totalFrozen) return;

                _frozenTotal = _total.Expected;
                _totalFrozen = true;
            }
        }
    }
}
