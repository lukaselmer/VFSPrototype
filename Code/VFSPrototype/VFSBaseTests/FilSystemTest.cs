using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBaseTests.Helpers;

namespace VFSBaseTests
{
    [TestClass]
    public class FilSystemTest
    {
        private const string DefaultTestfileFolder = "../../../Testfiles/FileSystemTests/";
        private readonly long _defaultSize = MathUtil.MB(5);

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

        private IFileSystem CreateFileSystem()
        {
            return CreateFileSystem(RandomTestfilePath());
        }

        private IFileSystem CreateFileSystem(string path)
        {
            return FileSystemFactory.Create(TestHelper.CreateFileSystemOptions(path, _defaultSize), "");
        }

        private IFileSystem ImportFileSystem(string path)
        {
            return FileSystemFactory.Import(path, "");
        }

        private static string RandomTestfilePath()
        {
            return Path.Combine(DefaultTestfileFolder, Guid.NewGuid() + ".vhs");
        }

        [TestMethod]
        public void TestCreateTopLevelFolder()
        {
            using (var fs = CreateFileSystem())
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, "test");
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }

            using (var fs = CreateFileSystem())
            {
                //TODO: make this true! Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }
        }

        [TestMethod]
        public void TestNamesShouldSupportUtf8()
        {
            const string name = "∀α,β∈∑α≤β∧β≥α=>α=β";
            var path = RandomTestfilePath();
            using (var fs = CreateFileSystem(path))
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, name);
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
                fs.Dispose();
            }

            using (var fs = ImportFileSystem(path))
            {
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
                Assert.IsTrue(fs.Folders(fs.Root).First().Name == name);
            }
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestRootFolderSetNotPossible()
        {
            new RootFolder().BlockNumber = 42;
        }
    }
}
