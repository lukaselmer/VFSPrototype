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
        public void TestCompressionLz77Strategy()
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

                Assert.IsFalse(c.CanSeek);
            }
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestCompressionLz77StrategyFail1()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms) as SelfMadeLz77Stream;
                Assert.IsNotNull(a);
                a.Seek(0, SeekOrigin.Begin);
            }
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestCompressionLz77StrategyFail2()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms) as SelfMadeLz77Stream;
                Assert.IsNotNull(a);
                a.SetLength(0);
            }
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestCompressionLz77StrategyFail3()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms) as SelfMadeLz77Stream;
                Assert.IsNotNull(a);
                var x = a.Length;
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestCompressionLz77StrategyFail4()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms) as SelfMadeLz77Stream;
                a.Write(null, 0, 0);
                Assert.IsNotNull(a);
                var x = a.Length;
                Assert.AreEqual(0, x);
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestCompressionLz77StrategyFail5()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms) as SelfMadeLz77Stream;
                a.Read(null, 0, 0);
                Assert.IsNotNull(a);
                var x = a.Length;
                Assert.AreEqual(0, x);
            }
        }

        [TestMethod]
        public void TestCompressionLz77StrategyFail6()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.None, StreamCompressionType.SelfMadeLz77));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms) as SelfMadeLz77Stream;
                Assert.AreEqual(0, a.Read(new byte[10], -1, 0));
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
