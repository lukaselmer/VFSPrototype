using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBaseTests.Helpers;

namespace VFSBaseTests.History
{
    [TestClass]
    public class HistoryTest
    {
        private TestHelper _testHelper;
        private const string DefaultTestfileFolder = "../../../Testfiles/HistoryTests/";

        [TestInitialize]
        public void CreateTestFolder()
        {
            _testHelper = new TestHelper(DefaultTestfileFolder);
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            _testHelper.CleanupTestFolder();
        }

        [TestMethod]
        public void TestFolderVersions()
        {
            using (var m = _testHelper.GetManipulator())
            {
                Assert.AreEqual(0, m.Version("/"));
                m.CreateFolder("test");
                Assert.AreEqual(1, m.Version("/"));
                Assert.AreEqual(1, m.Version("test"));
                m.CreateFolder("test/blub");
                Assert.AreEqual(2, m.Version("/"));
                Assert.AreEqual(2, m.Version("test"));
                Assert.AreEqual(2, m.Version("test/blub"));
                m.Delete("test/blub");
                Assert.AreEqual(3, m.Version("/"));
                Assert.AreEqual(3, m.Version("test"));

                _testHelper.AssertSequenceEqual(new long[] { 0, 1, 2, 3 }, m.Versions("/"));
                _testHelper.AssertSequenceEqual(new long[] { 1, 2, 3 }, m.Versions("test"));
            }
        }

        [TestMethod]
        public void TestFolderHistory()
        {
            using (var m = _testHelper.GetManipulator())
            {
                m.CreateFolder("test"); // V1
                m.CreateFolder("test/blub"); // V2
                m.CreateFolder("test/bla"); // V3
                m.Delete("test/blub"); // V4
                m.CreateFolder("test/blub"); // V5

                _testHelper.AssertSequenceEqual(new string[0], m.Folders("test", 1));
                _testHelper.AssertSequenceEqual(new[] { "blub" }, m.Folders("test", 2));
                _testHelper.AssertSequenceEqual(new[] { "blub", "bla" }, m.Folders("test", 3));
                _testHelper.AssertSequenceEqual(new[] { "bla" }, m.Folders("test", 4));
                _testHelper.AssertSequenceEqual(new[] { "blub", "bla" }, m.Folders("test", 5));
            }
        }

        [TestMethod]
        public void TestFileHistoryExport()
        {
            using (var m = _testHelper.GetManipulator())
            {
                var pathToTestfile = _testHelper.RandomTestfilePath();

                var internalTestfilePath = Path.GetFileName(pathToTestfile);

                ImportFile(pathToTestfile, internalTestfilePath, "xxx", m); // V1
                File.Delete(pathToTestfile);
                m.Delete(internalTestfilePath); // V2

                ImportFile(pathToTestfile, internalTestfilePath, "yyy", m);  // V3
                File.Delete(pathToTestfile);
                m.Delete(internalTestfilePath); // V4

                AssertExportThrowsException(m, internalTestfilePath, 0);
                AssertExportThrowsException(m, internalTestfilePath, 2);
                AssertExportThrowsException(m, internalTestfilePath, 4);

                m.Export(internalTestfilePath, pathToTestfile + "x", null, 1);
                m.Export(internalTestfilePath, pathToTestfile + "y", null, 3);

                Assert.AreEqual("xxx", File.ReadAllText(pathToTestfile + "x"));
                Assert.AreEqual("yyy", File.ReadAllText(pathToTestfile + "y"));
            }
        }

        [TestMethod]
        public void TestSwitchToVersion()
        {
            using (var m = _testHelper.GetManipulator())
            {
                m.CreateFolder("test"); // V1
                m.CreateFolder("test/blub"); // V2
                m.CreateFolder("test/bla"); // V3
                m.Delete("test/blub"); // V4
                m.CreateFolder("test/blub"); // V5

                m.SwitchToVersion(0);
                Assert.AreEqual(0, m.Folders("/").Count);
                Assert.AreEqual(0, m.Version("/"));

                m.SwitchToVersion(2);
                Assert.IsTrue(m.Exists("test/blub"));
                Assert.AreEqual(2, m.Version("/"));

                m.SwitchToVersion(4);
                Assert.IsFalse(m.Exists("test/blub"));
                Assert.AreEqual(4, m.Version("/"));

                m.SwitchToVersion(5);
                Assert.IsTrue(m.Exists("test/blub"));
                Assert.AreEqual(5, m.Version("/"));
            }
        }

        [TestMethod]
        public void TestSwitchToLatestVersion()
        {
            using (var m = _testHelper.GetManipulator())
            {
                m.CreateFolder("test"); // V1
                m.CreateFolder("test/blub"); // V2
                m.CreateFolder("test/bla"); // V3
                m.Delete("test/blub"); // V4
                m.CreateFolder("test/blub"); // V5

                m.SwitchToVersion(3);
                Assert.AreEqual(3, m.Version("/"));

                m.SwitchToLatestVersion();
                Assert.AreEqual(5, m.Version("/"));
            }
        }

        private static void AssertExportThrowsException(IFileSystemTextManipulator m, string testFileSource, int version)
        {
            try
            {
                m.Export(testFileSource, testFileSource, null, version);
                Assert.Fail("Should throw exception");
            }
            catch (VFSException)
            {
                // Pass
            }
        }

        private static void ImportFile(string testFileSource, string internalTestfilePath, string testFileData, IFileSystemTextManipulator m)
        {
            if (File.Exists(testFileSource)) File.Delete(testFileSource);
            File.WriteAllText(testFileSource, testFileData);
            m.Import(testFileSource, internalTestfilePath);
        }
    }
}
