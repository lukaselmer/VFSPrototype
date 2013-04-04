using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Persistence;
using VFSBase.Persistence.Blocks;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockParserTest
    {
        [TestMethod]
        public void TestParseEmptyDirectoryBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);
            var b1 = new byte[1024];
            Assert.AreSame(EmptyBlock.Get(), b.BytesToNode(b1));

            var b2 = new byte[options.BlockSize];
            Assert.AreSame(EmptyBlock.Get(), b.BytesToNode(b2));
        }

        [TestMethod]
        public void TestParseDirectoryBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            var bb = new byte[options.BlockSize];
            bb[0] = 1;
            Assert.IsInstanceOfType(b.BytesToNode(bb), typeof(Folder));
        }

        [TestMethod]
        public void TestParseFileBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            var bb = new byte[options.BlockSize];

            bb[0] = 2;
            Assert.IsInstanceOfType(b.BytesToNode(bb), typeof(VFSFile));
        }

        [TestMethod]
        public void TestWriteFolderBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            var f = new Folder("blubα");

            var bb = b.NodeToBytes(f);

            Assert.AreEqual(0x1, bb[0]);
        }

        [TestMethod]
        public void TestWriteFileBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            var f = new VFSFile("blubα", new byte[1]);

            var bb = b.NodeToBytes(f);

            Assert.AreEqual(0x2, bb[0]);
        }

        [TestMethod]
        public void TestWriteAndPareseFolderBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            const string name = "blubα";

            var f1 = new Folder(name);

            var bb = b.NodeToBytes(f1);
            var f2 = b.BytesToNode(bb);

            Assert.AreEqual(name, f2.Name);
            Assert.IsInstanceOfType(f2, typeof(Folder));
        }
    }
}
