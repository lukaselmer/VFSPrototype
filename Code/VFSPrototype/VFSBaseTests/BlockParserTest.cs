using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Persistence;
using VFSBase.Persistence.Blocks;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockParserTest
    {
        [ExpectedException(typeof (VFSException))]
        [TestMethod]
        public void TestParseTooSmallDirectoryBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);
            var b1 = new byte[1024];
            b.BytesToNode(b1);
        }

        
        [ExpectedException(typeof (VFSException))]
        [TestMethod]
        public void TestParseEmptyDirectoryBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);
            var b2 = new byte[options.BlockSize];
            b.BytesToNode(b2);
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

            var f = new VFSFile("αaαaαaαablubα");

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
