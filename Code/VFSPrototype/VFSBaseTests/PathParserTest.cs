using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;

namespace VFSBaseTests
{
    [TestClass]
    public class PathParserTest
    {
        [TestMethod]
        public void TestNormalizePath()
        {
            Assert.AreEqual("a/b", PathParser.NormalizePath("/a/b"));
            // TODO: enhance this!
        }
    }
}
