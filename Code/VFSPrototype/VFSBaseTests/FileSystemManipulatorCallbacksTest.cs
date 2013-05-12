using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Callbacks;
using VFSBase.Factories;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBaseTests.Helpers;

namespace VFSBaseTests
{
    [TestClass]
    public class FileSystemManipulatorCallbacksTest
    {
        private TestHelper _testHelper;
        private const string DefaultTestfileFolderPath = "../../../Testfiles/FileSystemManipulatorCallbacksTest";
        private const string DummyImportFolderPath = "../../../Testfiles/DummyfolderImport";
        private const string DummyExportFolderPath = "../../../Testfiles/DummyfolderExport";

        private IFileSystemTextManipulator InitTestFileSystemManipulator()
        {
            return _testHelper.GetManipulator();
        }

        [TestInitialize]
        public void CreateDummyfiles()
        {
            _testHelper = new TestHelper(DefaultTestfileFolderPath);

            if (Directory.Exists(DummyImportFolderPath)) Directory.Delete(DummyImportFolderPath, true);
            if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);
            Directory.CreateDirectory(DummyImportFolderPath);
            File.WriteAllText(DummyImportFolderPath + "/a", "bli");
            File.WriteAllText(DummyImportFolderPath + "/b", "bla");
            File.WriteAllText(DummyImportFolderPath + "/c", "blub");
        }

        [TestCleanup]
        public void RemoveDummyfiles()
        {
            if (Directory.Exists(DummyImportFolderPath)) Directory.Delete(DummyImportFolderPath, true);
            if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);
            
            _testHelper.CleanupTestFolder();
        }

        [TestMethod]
        public void TestImportAbort()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                var completed = false;
                var success = false;

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

                var totalCounter = new CountTester(4);
                Action<int> testTotalToProcess = totalCounter.Up;
                Action<int> testCurrentlyProcessed = new CountTester(4, totalCounter).Up;

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks(() => false, b => { completed = true; success = b; }, testTotalToProcess, testCurrentlyProcessed));
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
                if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);

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
                if (Directory.Exists(DummyExportFolderPath)) Directory.Delete(DummyExportFolderPath, true);

                var completed = false;
                var success = false;

                var totalCounter = new CountTester(4);
                Action<int> testTotalToProcess = totalCounter.Up;
                Action<int> testCurrentlyProcessed = new CountTester(4, totalCounter).Up;

                m.Import(DummyImportFolderPath, "dummy");
                m.Export("dummy", DummyExportFolderPath, new ExportCallbacks(() => false, b => { completed = true; success = b; }, testTotalToProcess, testCurrentlyProcessed));
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
                var completed = false;
                var success = false;

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks());
                m.Copy("dummy", "dummy2", new CopyCallbacks(() => true, b => { completed = true; success = b; }));
                Assert.IsFalse(m.Exists("dummy2"));

                Assert.AreEqual(completed, true);
                Assert.AreEqual(success, false);
            }
        }

        [TestMethod]
        public void TestCopyCallbacks()
        {
            using (var m = InitTestFileSystemManipulator())
            {
                var completed = false;
                var success = false;

                var totalCounter = new CountTester(4);
                Action<int> testTotalToProcess = totalCounter.Up;
                Action<int> testCurrentlyProcessed = new CountTester(4, totalCounter).Up;

                m.Import(DummyImportFolderPath, "dummy", new ImportCallbacks());
                m.Copy("dummy", "dummy2", new CopyCallbacks(() => false, b => { completed = true; success = b; }, testTotalToProcess, testCurrentlyProcessed));
                Assert.IsTrue(m.Exists("dummy2"));

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

            private int Expected { get; set; }

            public void Up(int actual)
            {
                if (_total != null)
                {
                    FreezeTotal();
                    Assert.AreEqual(_frozenTotal, _total.Expected);
                }

                if (actual > _max) Assert.Fail("Too many calls");
                Assert.IsTrue(actual >= Expected);
                Expected = actual;
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
