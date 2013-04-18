using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence;

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

        [TestMethod]
        public void TestPower()
        {
            Assert.AreEqual(1, BinaryMathUtil.Power(2, 0));
            Assert.AreEqual(2, BinaryMathUtil.Power(2, 1));
            Assert.AreEqual(4, BinaryMathUtil.Power(2, 2));
            Assert.AreEqual(8, BinaryMathUtil.Power(2, 3));
            Assert.AreEqual(16, BinaryMathUtil.Power(2, 4));
            Assert.AreEqual(32, BinaryMathUtil.Power(2, 5));

            Assert.AreEqual(1, BinaryMathUtil.Power(3, 0));
            Assert.AreEqual(3, BinaryMathUtil.Power(3, 1));
            Assert.AreEqual(3 * 3, BinaryMathUtil.Power(3, 2));
            Assert.AreEqual(3 * 3 * 3, BinaryMathUtil.Power(3, 3));
            Assert.AreEqual(3 * 3 * 3 * 3, BinaryMathUtil.Power(3, 4));
            Assert.AreEqual(3 * 3 * 3 * 3 * 3, BinaryMathUtil.Power(3, 5));
        }

        [TestMethod]
        public void TestKB()
        {
            Assert.AreEqual(BinaryMathUtil.Power2(10), BinaryMathUtil.KB(1));
            Assert.AreEqual(BinaryMathUtil.Power2(11), BinaryMathUtil.KB(2));
            Assert.AreEqual(BinaryMathUtil.Power2(10) * 3, BinaryMathUtil.KB(3));
        }

        [TestMethod]
        public void TestMB()
        {
            Assert.AreEqual(BinaryMathUtil.Power2(20), BinaryMathUtil.MB(1));
            Assert.AreEqual(BinaryMathUtil.Power2(21), BinaryMathUtil.MB(2));
            Assert.AreEqual(BinaryMathUtil.Power2(20) * 3, BinaryMathUtil.MB(3));
        }

        [TestMethod]
        public void TestGB()
        {
            Assert.AreEqual(BinaryMathUtil.Power2(30), BinaryMathUtil.GB(1));
            Assert.AreEqual(BinaryMathUtil.Power2(31), BinaryMathUtil.GB(2));
            Assert.AreEqual(BinaryMathUtil.Power2(30) * 3, BinaryMathUtil.GB(3));
        }

        [TestMethod]
        public void TestMBAndKBEquality()
        {
            Assert.AreEqual(BinaryMathUtil.KB(1) * BinaryMathUtil.KB(1), BinaryMathUtil.MB(1));
            Assert.AreEqual(BinaryMathUtil.KB(5) * BinaryMathUtil.KB(1), BinaryMathUtil.MB(5));
            Assert.AreEqual(BinaryMathUtil.KB(2) * BinaryMathUtil.KB(2), BinaryMathUtil.MB(4));
        }

        [TestMethod]
        public void TestMbKbGbEquality()
        {
            Assert.AreEqual(BinaryMathUtil.MB(1) * BinaryMathUtil.KB(1), BinaryMathUtil.GB(1));
            Assert.AreEqual(BinaryMathUtil.MB(5) * BinaryMathUtil.KB(1), BinaryMathUtil.GB(5));
            Assert.AreEqual(BinaryMathUtil.MB(2) * BinaryMathUtil.KB(2), BinaryMathUtil.GB(4));
        }
    }
}
