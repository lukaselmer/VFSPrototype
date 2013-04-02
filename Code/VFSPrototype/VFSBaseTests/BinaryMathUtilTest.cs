using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistance;

namespace VFSBaseTests
{
    [TestClass]
    public class BinaryMathUtilTest
    {
        [TestMethod]
        public void TestPower2()
        {
            Assert.AreEqual(1, BinaryMathUtil.Power2(0));
            Assert.AreEqual(2, BinaryMathUtil.Power2(1));
            Assert.AreEqual(4, BinaryMathUtil.Power2(2));
            Assert.AreEqual(8, BinaryMathUtil.Power2(3));
            Assert.AreEqual(16, BinaryMathUtil.Power2(4));
            Assert.AreEqual(32, BinaryMathUtil.Power2(5));
            Assert.AreEqual(64, BinaryMathUtil.Power2(6));
            Assert.AreEqual(1024, BinaryMathUtil.Power2(10));
            Assert.AreEqual(1024 * 1024, BinaryMathUtil.Power2(20));
            Assert.AreEqual(1024 * 1024 * 1024, BinaryMathUtil.Power2(30));
            Assert.AreEqual(1024L * 1024L * 1024L * 1024L, BinaryMathUtil.Power2(40));
        }
    }
}
