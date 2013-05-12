using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence.Coding.SelfMadeLz77;

namespace VFSBaseTests.Coding
{
    [TestClass]
    public class CompressionTest
    {
        [TestMethod]
        public void TestLz77()
        {
            var r = new Random(3);
            var expected = new byte[3000];
            var actual = new byte[3000];
            r.NextBytes(expected);

            using (var ms = new MemoryStream())
            {
                var s1 = new SelfMadeLz77Stream(ms, CompressionMode.Compress);
                var pos1 = 0;
                while (pos1 < expected.Length)
                {
                    var count = Math.Min(r.Next(1, 1024), actual.Length - pos1);
                    s1.Write(expected, pos1, count);
                    pos1 += count;
                }
                s1.Flush();

                ms.Flush();
                ms.Position = 0;

                var s2 = new SelfMadeLz77Stream(ms, CompressionMode.Decompress);
                var pos2 = 0;
                while (pos2 < expected.Length)
                {
                    var count = Math.Min(r.Next(1, 1024), actual.Length - pos2);
                    if (count == 0) break;

                    var readActual = s2.Read(actual, pos2, count);
                    pos2 += readActual;

                    if (readActual == 0) break;
                }

                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.AreEqual(expected[i], actual[i]);
                }
            }
        }
    }
}
