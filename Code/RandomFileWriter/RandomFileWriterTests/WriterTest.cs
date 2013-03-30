using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RandomFileWriter;

namespace RandomFileWriterTests
{
    [TestClass]
    public class WriterTest
    {
        private const string Big = "../../../testfiles/bigfile.vhs";
        private const string Small = "../../../testfiles/small.vhs";
        private const string Same = "../../../testfiles/bigfile_same.vhs";
        private const string Bigger = "../../../testfiles/bigfile_bigger.vhs";

        [TestMethod]
        public void TestRandomWriteBeginning()
        {
            if (File.Exists(Big)) File.Delete(Big);
            Assert.IsFalse(File.Exists(Big));

            using (var w = new Writer(Big))
            {
                w.WriteFile(Small, 0);
            }

            Assert.IsTrue(File.Exists(Big), String.Format("File {0} should exist", Big));
            Assert.IsTrue(File.ReadAllText(Big).StartsWith("1234"));
        }

        [TestMethod]
        public void TestRandomWriteAfterBeginning()
        {
            if (File.Exists(Big)) File.Delete(Big);
            Assert.IsFalse(File.Exists(Big));

            using (var w = new Writer(Big))
            {
                w.WriteFile(Small, 1);
            }

            Assert.IsTrue(File.Exists(Big), String.Format("File {0} should exist", Big));
            var x = File.ReadAllText(Big);
            Assert.IsTrue(File.ReadAllText(Big).StartsWith("\01234"));
        }

        [TestMethod]
        public void TestRandomWriteSomewhere()
        {
            if (File.Exists(Big)) File.Delete(Big);
            Assert.IsFalse(File.Exists(Big));

            using (var w = new Writer(Big))
            {
                w.WriteFile(Small, 3);
            }

            Assert.IsTrue(File.Exists(Big), String.Format("File {0} should exist", Big));
            var x = File.ReadAllText(Big);
            Assert.IsTrue(File.ReadAllText(Big).StartsWith("\0\0\01234"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooBigFile()
        {
            if (File.Exists(Big)) File.Delete(Big);
            Assert.IsFalse(File.Exists(Big));

            using (var w = new Writer(Big))
            {
                w.WriteFile(Bigger, 0); // this should throw an exception!
            }
        }

        [TestMethod]
        public void TestSmallerBigFile()
        {
            if (File.Exists(Big)) File.Delete(Big);
            Assert.IsFalse(File.Exists(Big));

            using (var w = new Writer(Big))
            {
                w.WriteFile(Same, 0); // this should just work
            }

            Assert.IsTrue(File.Exists(Big), String.Format("File {0} should exist", Big));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSmallerBigFileFail()
        {
            if (File.Exists(Big)) File.Delete(Big);
            Assert.IsFalse(File.Exists(Big));

            using (var w = new Writer(Big))
            {
                w.WriteFile(Same, 1); // one byte overflow => this should throw an exception!
            }

            Assert.IsTrue(File.Exists(Big), String.Format("File {0} should exist", Big));
        }
    }
}
