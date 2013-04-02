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
            Assert.AreEqual("a/b", PathParser.NormalizePath("a/b/"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a////b/"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a/////b/"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a//////b/"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a///////b/"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a////////b/"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a////////b/ //"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a///// ///b/ //"));
            Assert.AreEqual("a/b", PathParser.NormalizePath(" a///// ///b/ //"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a / //// ///b/ //"));
            Assert.AreEqual("a/b", PathParser.NormalizePath(" a/b"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a/b "));
            Assert.AreEqual("a/b", PathParser.NormalizePath("//a/b "));
            Assert.AreEqual("a/b", PathParser.NormalizePath("///   / //  /  a/ b    "));
            Assert.AreEqual("a/b/c/d", PathParser.NormalizePath("///   / //  /  a/ b   //////c//  // d /// //// /// "));
        }
    }
}
