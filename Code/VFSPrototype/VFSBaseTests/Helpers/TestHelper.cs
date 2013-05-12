using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Factories;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence.Coding.General;

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

        public static FileSystemOptions CreateFileSystemOptions(string location)
        {
            return new FileSystemOptions(location, StreamEncryptionType.None, StreamCompressionType.None);
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
            return FileSystemFactory.Create(CreateFileSystemOptions(RandomTestfilePath()), "");
        }

        internal IFileSystemTextManipulator GetManipulator()
        {
            return GetManipulator(RandomTestfilePath());
        }

        private static IFileSystemTextManipulator GetManipulator(string path)
        {
            return new FileSystemTextManipulatorFactory().CreateFileSystemTextManipulator(CreateFileSystemOptions(path), "");
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
