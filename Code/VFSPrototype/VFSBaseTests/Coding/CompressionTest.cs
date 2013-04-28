using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;

namespace VFSBaseTests.Coding
{
    [TestClass]
    public class CompressionTest
    {
        [TestMethod]
        public void TestLz77()
        {
            var r = new Random(1);
            var expected = new byte[10000];
            var actual = new byte[10000];
            r.NextBytes(expected);

            using (var ms = new MemoryStream())
            {
                using (var s = new SelfMadeLz77Stream(ms, CompressionMode.Compress))
                {
                    var pos = 0;
                    while (pos < expected.Length)
                    {
                        var count = r.Next(1024);
                        s.Write(expected, pos, count);
                        pos += count;
                    }
                }

                ms.Position = 0;

                using (var s = new SelfMadeLz77Stream(ms, CompressionMode.Decompress))
                {
                    var pos = 0;
                    while (pos < expected.Length)
                    {
                        var count = r.Next(1024);
                        s.Read(actual, pos, count);
                        pos += count;
                    }
                }

                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.AreEqual(expected[i], actual[i]);
                }
            }
        }
    }
}
