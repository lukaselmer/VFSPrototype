using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.SelfMadeLz77;
using VFSBase.Persistence.Coding.Strategies;

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

        [TestMethod]
        public void TestCompressionStrategy()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms);
                var b = s.DecorateToHost(ms);
                Assert.AreNotSame(ms, a);
                Assert.AreNotSame(ms, b);
                Assert.IsTrue(a is SelfMadeLz77Stream);
                Assert.IsTrue(b is SelfMadeLz77Stream);
                var c = a as SelfMadeLz77Stream;
                var d = b as SelfMadeLz77Stream;
                Assert.AreEqual(false, c.CanRead);
                Assert.AreEqual(true, c.CanWrite);
                Assert.AreEqual(true, d.CanRead);
                Assert.AreEqual(false, d.CanWrite);
            }
        }

        [TestMethod]
        public void TestMicrosoftDeflateCompressionStrategy()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.MicrosoftDeflate));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms);
                var b = s.DecorateToHost(ms);
                Assert.AreNotSame(ms, a);
                Assert.AreNotSame(ms, b);
                Assert.IsTrue(a is DeflateStream);
                Assert.IsTrue(b is DeflateStream);
                var c = a as DeflateStream;
                var d = b as DeflateStream;
                Assert.AreEqual(false, c.CanRead);
                Assert.AreEqual(true, c.CanWrite);
                Assert.AreEqual(true, d.CanRead);
                Assert.AreEqual(false, d.CanWrite);
            }
        }

    }
}
