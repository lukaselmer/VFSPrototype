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
            Assert.AreEqual(1, MathUtil.Power2(0));
            Assert.AreEqual(2, MathUtil.Power2(1));
            Assert.AreEqual(4, MathUtil.Power2(2));
            Assert.AreEqual(8, MathUtil.Power2(3));
            Assert.AreEqual(16, MathUtil.Power2(4));
            Assert.AreEqual(32, MathUtil.Power2(5));
            Assert.AreEqual(64, MathUtil.Power2(6));
            Assert.AreEqual(1024, MathUtil.Power2(10));
            Assert.AreEqual(1024 * 1024, MathUtil.Power2(20));
            Assert.AreEqual(1024 * 1024 * 1024, MathUtil.Power2(30));
            Assert.AreEqual(1024L * 1024L * 1024L * 1024L, MathUtil.Power2(40));
        }

        [TestMethod]
        public void TestPower()
        {
            Assert.AreEqual(1, MathUtil.Power(2, 0));
            Assert.AreEqual(2, MathUtil.Power(2, 1));
            Assert.AreEqual(4, MathUtil.Power(2, 2));
            Assert.AreEqual(8, MathUtil.Power(2, 3));
            Assert.AreEqual(16, MathUtil.Power(2, 4));
            Assert.AreEqual(32, MathUtil.Power(2, 5));

            Assert.AreEqual(1, MathUtil.Power(3, 0));
            Assert.AreEqual(3, MathUtil.Power(3, 1));
            Assert.AreEqual(3 * 3, MathUtil.Power(3, 2));
            Assert.AreEqual(3 * 3 * 3, MathUtil.Power(3, 3));
            Assert.AreEqual(3 * 3 * 3 * 3, MathUtil.Power(3, 4));
            Assert.AreEqual(3 * 3 * 3 * 3 * 3, MathUtil.Power(3, 5));
        }

        [TestMethod]
        public void TestKB()
        {
            Assert.AreEqual(MathUtil.Power2(10), MathUtil.KB(1));
            Assert.AreEqual(MathUtil.Power2(11), MathUtil.KB(2));
            Assert.AreEqual(MathUtil.Power2(10) * 3, MathUtil.KB(3));
        }

        [TestMethod]
        public void TestMB()
        {
            Assert.AreEqual(MathUtil.Power2(20), MathUtil.MB(1));
            Assert.AreEqual(MathUtil.Power2(21), MathUtil.MB(2));
            Assert.AreEqual(MathUtil.Power2(20) * 3, MathUtil.MB(3));
        }

        [TestMethod]
        public void TestGB()
        {
            Assert.AreEqual(MathUtil.Power2(30), MathUtil.GB(1));
            Assert.AreEqual(MathUtil.Power2(31), MathUtil.GB(2));
            Assert.AreEqual(MathUtil.Power2(30) * 3, MathUtil.GB(3));
        }

        [TestMethod]
        public void TestMBAndKBEquality()
        {
            Assert.AreEqual(MathUtil.KB(1) * MathUtil.KB(1), MathUtil.MB(1));
            Assert.AreEqual(MathUtil.KB(5) * MathUtil.KB(1), MathUtil.MB(5));
            Assert.AreEqual(MathUtil.KB(2) * MathUtil.KB(2), MathUtil.MB(4));
        }

        [TestMethod]
        public void TestMbKbGbEquality()
        {
            Assert.AreEqual(MathUtil.MB(1) * MathUtil.KB(1), MathUtil.GB(1));
            Assert.AreEqual(MathUtil.MB(5) * MathUtil.KB(1), MathUtil.GB(5));
            Assert.AreEqual(MathUtil.MB(2) * MathUtil.KB(2), MathUtil.GB(4));
        }
    }
}
