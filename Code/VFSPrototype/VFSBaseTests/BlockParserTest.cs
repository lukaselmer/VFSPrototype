using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistance;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockParserTest
    {
        [TestMethod]
        public void TestParseDirectoryBlock()
        {
            var b = new BlockParser();
            var bb = new byte[1024];
            b.ParseBlock(bb);
        }
    }
}
