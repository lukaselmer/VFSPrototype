using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence;

namespace VFSBaseTests
{
    [TestClass]
    public class PositionCalculatorTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            var s = PositionCalculatorSettings();
            var p = new PositionCalculator(s);
            Assert.AreEqual(s, p.Settings);
        }

        private static PositionCalculatorSettings PositionCalculatorSettings()
        {
            var superBlockSize = BinaryMathUtil.MB(1);
            var blockSize = BinaryMathUtil.Power2(9);
            var s = new PositionCalculatorSettings(superBlockSize, blockSize);
            return s;
        }

        [TestMethod]
        public void TestStartBlock()
        {
            var s = PositionCalculatorSettings();
            var p = new PositionCalculator(s);
            Assert.AreEqual(s.SuperBlockSize, p.StartBlock);
        }

        [TestMethod]
        public void TestStartBlockAtPosition0()
        {
            var s = PositionCalculatorSettings();
            var p = new PositionCalculator(s);
            Assert.AreEqual(p.CalculateBlockStart(0), p.StartBlock);
        }

        [TestMethod]
        public void TestBlockPositionCalculation()
        {
            var s = PositionCalculatorSettings();
            var p = new PositionCalculator(s);
            Assert.AreEqual(p.CalculateBlockStart(0), p.StartBlock);
            Assert.AreEqual(p.CalculateBlockStart(1), s.SuperBlockSize + s.BlockSize);
            Assert.AreEqual(p.CalculateBlockStart(10), s.SuperBlockSize + 10 * s.BlockSize);
            Assert.AreEqual(p.CalculateBlockStart(3), s.SuperBlockSize + 3 * s.BlockSize);
        }
    }
}
