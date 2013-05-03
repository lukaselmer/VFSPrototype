using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBaseTests.Helpers;

namespace VFSBaseTests
{
    [TestClass]
    public class BlockListTest
    {
        /*[TestMethod]
        public void TestEmptyFolder()
        {
            IIndexNode n = new Folder("test");
            n.IndirectNodeNumber = 0;
            var b = new BlockList(n, null, TestHelper.CreateFileSystemOptions("", 0), null, null, null);
            Assert.AreEqual(0, b.Blocks().Count());
            b.Remove(new VFSFile("xxx"), false);
        }*/

        /*[ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestRemoveNull()
        {
            IIndexNode n = new Folder("test");
            n.IndirectNodeNumber = 0;
            var b = new BlockList(n, null, TestHelper.CreateFileSystemOptions("", 0), null, null, null);
            b.Remove(null, false);
        }*/

        /*[ExpectedException(typeof(VFSException))]
        [TestMethod]
        public void TestInvalidNode()
        {
            IIndexNode n = new VFSFile("test");
            n.IndirectNodeNumber = 0;
            var b = new BlockList(n, null, TestHelper.CreateFileSystemOptions("", 0), null, null, null);
            b.Remove(new VFSFile("xxx"), false);
        }*/
    }
}
