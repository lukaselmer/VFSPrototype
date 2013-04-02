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
        public void TestParseDirectoryBlock()
        {
            var b = new BlockParser(new FileSystemOptions("", 0));
            var bb = new byte[1024];
            Assert.AreEqual(new EmptyBlock(), b.ParseBlock(bb));
        }
    }
}
