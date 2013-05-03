using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBaseTests.Helpers;

namespace VFSBaseTests
{
    [TestClass]
    public class HistoryTest
    {
        private const string DefaultTestfileFolder = "../../../Testfiles/HistoryTests/";



        [TestInitialize]
        public void CreateTestFolder()
        {
            Directory.CreateDirectory(DefaultTestfileFolder);
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            Directory.Delete(DefaultTestfileFolder, true);
        }

        private FileSystemTextManipulator GetManipulator()
        {
            return GetManipulator(RandomTestfilePath());
        }

        private FileSystemTextManipulator GetManipulator(string path)
        {
            return new FileSystemTextManipulator(TestHelper.CreateFileSystemOptions(path, 0), "");
        }

        private static string RandomTestfilePath()
        {
            return Path.Combine(DefaultTestfileFolder, Guid.NewGuid() + ".vhs");
        }

        [TestMethod]
        public void TestFolderVersions()
        {
            using (var m = GetManipulator())
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

                AssertSequenceEqual(new long[] { 0, 1, 2, 3 }, m.Versions("/"));
                AssertSequenceEqual(new long[] { 1, 2, 3 }, m.Versions("test"));
            }
        }

        [TestMethod]
        public void TestFolderHistory()
        {
            using (var m = GetManipulator())
            {
                m.CreateFolder("test"); // V1
                m.CreateFolder("test/blub"); // V2
                m.CreateFolder("test/bla"); // V3
                m.Delete("test/blub"); // V4
                m.CreateFolder("test/blub"); // V5

                AssertSequenceEqual(new string[0], m.Folders("test", 1));
                AssertSequenceEqual(new[] { "blub" }, m.Folders("test", 2));
                AssertSequenceEqual(new[] { "blub", "bla" }, m.Folders("test", 3));
                AssertSequenceEqual(new[] { "bla" }, m.Folders("test", 4));
                AssertSequenceEqual(new[] { "blub", "bla" }, m.Folders("test", 5));
            }
        }

        [TestMethod]
        public void TestFileHistoryExport()
        {
            using (var m = GetManipulator())
            {
                string testFileSource = DefaultTestfileFolder + Guid.NewGuid() + "_test";

                ImportFile(testFileSource, "xxx", m); // V1
                File.Delete(testFileSource);
                m.Delete(testFileSource); // V2

                ImportFile(testFileSource, "yyy", m);  // V3
                File.Delete(testFileSource);
                m.Delete(testFileSource); // V4

                AssertExportThrowsException(m, testFileSource, 0);
                AssertExportThrowsException(m, testFileSource, 2);
                AssertExportThrowsException(m, testFileSource, 4);

                m.Export(testFileSource, testFileSource + "x", null, 1);
                m.Export(testFileSource, testFileSource + "y", null, 3);

                Assert.AreEqual("xxx", File.ReadAllLines(testFileSource + "x"));
                Assert.AreEqual("yyy", File.ReadAllLines(testFileSource + "y"));
            }
        }

        private static void AssertExportThrowsException(FileSystemTextManipulator m, string testFileSource, int version)
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

        private static void ImportFile(string testFileSource, string testFileData, FileSystemTextManipulator m)
        {
            if (File.Exists(testFileSource)) File.Delete(testFileSource);
            File.WriteAllText(testFileSource, testFileData);
            m.Import(testFileSource, testFileSource);
        }

        private static void AssertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var expectedArray = expected as T[] ?? expected.ToArray();
            var actualArray = actual as T[] ?? actual.ToArray();
            Assert.AreEqual(expectedArray.Count(), actualArray.Count());
            foreach (var e in expectedArray)
            {
                Assert.IsTrue(actualArray.Contains(e));
            }
        }
    }
}
