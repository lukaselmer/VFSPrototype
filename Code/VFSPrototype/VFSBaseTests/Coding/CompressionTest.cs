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
                using (var s = new SelfMadeLz77Stream(ms, CompressionMode.Compress))
                {
                    var pos = 0;
                    while (pos < expected.Length)
                    {
                        var count = Math.Min(r.Next(1, 1024), actual.Length - pos);
                        s.Write(expected, pos, count);
                        pos += count;
                    }
                    s.Flush();
                }

                ms.Flush();
                ms.Position = 0;

                using (var s = new SelfMadeLz77Stream(ms, CompressionMode.Decompress))
                {
                    var pos = 0;
                    while (pos < expected.Length)
                    {
                        var count = Math.Min(r.Next(1, 1024), actual.Length - pos);
                        if (count == 0) break;
                        
                        var readActual = s.Read(actual, pos, count);
                        pos += readActual;

                        if(readActual == 0) break;
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
