using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBase.Persistence.Blocks;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockParserTest
    {
        [ExpectedException(typeof(VFSException))]
        [TestMethod]
        public void TestParseTooSmallDirectoryBlock()
        {
            var options = new FileSystemOptions("", 0);
            var b = new BlockParser(options);
            var b1 = new byte[1024];
            b.BytesToNode(b1);
        }


        [ExpectedException(typeof(VFSException))]
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


        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestInvalidNode()
        {
            var options = new FileSystemOptions("", 0);
            var p = new BlockParser(options);
            p.NodeToBytes(new MyType());
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestInvalidBlockSizeForFile()
        {
            var options = new FileSystemOptions("", 0);
            var p = new BlockParser(options);
            p.NodeToBytes(new MyType());
        }

        [ExpectedException(typeof(VFSException))]
        [TestMethod]
        public void TestNameTooLong()
        {
            var f = new VFSFile("0123456789");
            try
            {
                var p1 = new BlockParser(new FileSystemOptions("", 0) { NameLength = f.Name.Length });
                p1.NodeToBytes(f);
            }
            catch (VFSException)
            {
                Assert.Fail("Exception unexpected yet");
            }
            var p2 = new BlockParser(new FileSystemOptions("", 0) { NameLength = f.Name.Length - 1 });
            p2.NodeToBytes(f);
        }

        private class MyType : IIndexNode
        {
            public string Name { get; set; }
            public Folder Parent { get; set; }
            public long BlockNumber { get; set; }
            public long IndirectNodeNumber { get; set; }
            public long BlocksCount { get; set; }
        }
    }
}
