using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBaseTests.Helpers
{
    internal class TestHelper
    {
        private readonly string _testfileFolder;

        public TestHelper(string testfileFolder)
        {
            _testfileFolder = testfileFolder;
            PrepareTestFolder();
        }

        public static FileSystemOptions CreateFileSystemOptions(string location, long diskSize)
        {
            return new FileSystemOptions(location, diskSize, StreamEncryptionType.None, StreamCompressionType.None);
        }

        internal void PrepareTestFolder()
        {
            Directory.CreateDirectory(_testfileFolder);
        }

        internal void CleanupTestFolder()
        {
            Directory.Delete(_testfileFolder, true);
        }

        internal IFileSystem GetFileSystem()
        {
            return FileSystemFactory.Create(CreateFileSystemOptions(RandomTestfilePath(), 0), "");
        }

        internal IFileSystemTextManipulator GetManipulator()
        {
            return GetManipulator(RandomTestfilePath());
        }

        private static IFileSystemTextManipulator GetManipulator(string path)
        {
            return new FileSystemTextManipulatorFactory().CreateFileSystemTextManipulator(CreateFileSystemOptions(path, 0), "");
        }

        internal string RandomTestfilePath()
        {
            return Path.Combine(_testfileFolder, Guid.NewGuid() + ".vhs");
        }

        internal void AssertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
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
