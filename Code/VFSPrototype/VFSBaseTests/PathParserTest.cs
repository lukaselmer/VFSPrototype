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
            Assert.AreEqual("a/b", PathParser.NormalizePath("\0a/b"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a/b\0"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a/b\0\0"));
            Assert.AreEqual("a/b/c/d", PathParser.NormalizePath("\0a/b\0/c/d\0\0"));
            Assert.AreEqual("a/b", PathParser.NormalizePath(" a/b"));
            Assert.AreEqual("a/b", PathParser.NormalizePath("a/b "));
            Assert.AreEqual("a/b", PathParser.NormalizePath("//a/b "));
            Assert.AreEqual("a/b", PathParser.NormalizePath("///   / //  /  a/ b    "));
            Assert.AreEqual("a/b/c/d", PathParser.NormalizePath("///   / //  /  a/ b   //////c//  // d /// //// /// "));
            Assert.AreEqual("a/b/c/d", PathParser.NormalizePath("///   / //\0  /  \0a\0/ b\0   ///\0///c//  // d ///\0 //// /// "));
        }
    }
}
