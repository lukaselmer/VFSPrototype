using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistance;

namespace VFSBaseTests
{
    [TestClass]
    public class FilSystemTest
    {
        const string DefaultTestfilePath = "../../../Testfiles/testfile.vhs";
        private long _defaultSize = BinaryMathUtil.MB(5);

        [TestCleanup]
        public void RemoveTestfile()
        {
            File.Delete(DefaultTestfilePath);
        }


        private IFileSystem GetFileSystem(string path = DefaultTestfilePath)
        {
            var o = new FileSystemOptions(path, _defaultSize);
            return FileSystemFactory.CreateOrImport(o);
        }

        [TestMethod]
        public void TestCreateTopLevelFolder()
        {
            using (var fs = GetFileSystem())
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, new Folder("test"));
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
            var name = "∀α,β∈∑α≤β∧β≥α=>α=β";
            using (var fs = GetFileSystem())
            {
                Assert.IsTrue(!fs.Folders(fs.Root).Any());
                fs.CreateFolder(fs.Root, new Folder(name));
                Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
            }

            using (var fs = GetFileSystem())
            {
                // TODO: make this true! Assert.IsTrue(fs.Folders(fs.Root).Count() == 1);
                // TODO: make this true! Assert.IsTrue(fs.Folders(fs.Root).First().Name == name);
            }
        }
        
    }
}
