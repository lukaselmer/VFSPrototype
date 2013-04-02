using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RandomFileWriter;

namespace RandomFileWriterTests
{
    [TestClass]
    public class WriterTest
    {
        private const string Big = "../../../testfiles/bigfile_";
        private const string Small = "../../../testfiles/small.vhs";
        private const string Same = "../../../testfiles/bigfile_same.vhs";
        private const string Bigger = "../../../testfiles/bigfile_bigger.vhs";

        private static string RandomFilename()
        {
            return Big + Path.GetRandomFileName() + ".vhs";
        }

        [TestMethod]
        public void TestRandomWriteBeginning()
        {
            var filename = RandomFilename();
            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                Assert.IsFalse(File.Exists(filename));

                using (var w = new Writer(filename))
                {
                    w.WriteFile(Small, 0);
                }

                Assert.IsTrue(File.Exists(filename), String.Format("File {0} should exist", filename));
                Assert.IsTrue(File.ReadAllText(filename).StartsWith("1234"));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [TestMethod]
        public void TestRandomWriteAfterBeginning()
        {

            var filename = RandomFilename();

            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                Assert.IsFalse(File.Exists(filename));

                using (var w = new Writer(filename))
                {
                    w.WriteFile(Small, 1);
                }

                Assert.IsTrue(File.Exists(filename), String.Format("File {0} should exist", filename));
                var x = File.ReadAllText(filename);
                Assert.IsTrue(File.ReadAllText(filename).StartsWith("\01234"));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [TestMethod]
        public void TestRandomWriteSomewhere()
        {
            var filename = RandomFilename();
            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                Assert.IsFalse(File.Exists(filename));

                using (var w = new Writer(filename))
                {
                    w.WriteFile(Small, 3);
                }

                Assert.IsTrue(File.Exists(filename), String.Format("File {0} should exist", filename));
                var x = File.ReadAllText(filename);
                Assert.IsTrue(File.ReadAllText(filename).StartsWith("\0\0\01234"));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooBigFile()
        {
            var filename = RandomFilename();
            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                Assert.IsFalse(File.Exists(filename));

                using (var w = new Writer(filename))
                {
                    w.WriteFile(Bigger, 0); // this should throw an exception!
                }
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [TestMethod]
        public void TestSmallerBigFile()
        {
            var filename = RandomFilename();

            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                Assert.IsFalse(File.Exists(filename));

                using (var w = new Writer(filename))
                {
                    w.WriteFile(Same, 0); // this should just work
                }

                Assert.IsTrue(File.Exists(filename), String.Format("File {0} should exist", filename));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSmallerBigFileFail()
        {
            var filename = RandomFilename();
            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                Assert.IsFalse(File.Exists(filename));

                using (var w = new Writer(filename))
                {
                    w.WriteFile(Same, 1); // one byte overflow => this should throw an exception!
                }

                Assert.IsTrue(File.Exists(filename), String.Format("File {0} should exist", filename));
            }
            finally
            {
                File.Delete(filename);
            }
        }
    }
}
