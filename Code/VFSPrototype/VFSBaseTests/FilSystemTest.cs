using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence;

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

        private IFileSystem GetFileSystem()
        {
            return GetFileSystem(RandomTestfilePath());
        }

        private IFileSystem GetFileSystem(string path)
        {
            var o = new FileSystemOptions(path, _defaultSize);
            return FileSystemFactory.CreateOrImport(o, "");
        }

        private static string RandomTestfilePath()
        {
            return Path.Combine(DefaultTestfileFolder, Guid.NewGuid() + ".vhs");
        }

        [TestMethod]
        public void TestCreateTopLevelFolder()
        {
            using (var fs = GetFileSystem())
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, "test");
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }

            using (var fs = GetFileSystem())
            {
                //TODO: make this true! Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }
        }

        [TestMethod]
        public void TestNamesShouldSupportUtf8()
        {
            const string name = "∀α,β∈∑α≤β∧β≥α=>α=β";
            var path = RandomTestfilePath();
            using (var fs = GetFileSystem(path))
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, name);
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
                fs.Dispose();
            }

            using (var fs = GetFileSystem(path))
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
