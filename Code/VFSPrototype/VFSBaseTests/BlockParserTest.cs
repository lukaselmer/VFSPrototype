using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Persistance;
using VFSBase.Persistance.Blocks;

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
            Assert.AreSame(EmptyBlock.Get(), b.ParseBlock(b1));

            var b2 = new byte[options.BlockSize];
            Assert.AreSame(EmptyBlock.Get(), b.ParseBlock(b2));
        }

        [TestMethod]
        public void TestParseDirectoryBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            var bb = new byte[options.BlockSize];
            bb[0] = 1;
            Assert.IsInstanceOfType(b.ParseBlock(bb), typeof(Folder));
        }

        [TestMethod]
        public void TestParseFileBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);

            var bb = new byte[options.BlockSize];

            bb[0] = 2;
            Assert.IsInstanceOfType(b.ParseBlock(bb), typeof(VFSFile));
        }
    }
}
