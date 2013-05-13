using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VFSBlockAbstractionTests.Helpers
{
    internal class TestHelper
    {
        private readonly string _testfileFolder;

        public TestHelper(string testfileFolder)
        {
            _testfileFolder = testfileFolder;
            PrepareTestFolder();
        }

        internal void PrepareTestFolder()
        {
            Directory.CreateDirectory(_testfileFolder);
        }

        internal void CleanupTestFolder()
        {
            Directory.Delete(_testfileFolder, true);
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
