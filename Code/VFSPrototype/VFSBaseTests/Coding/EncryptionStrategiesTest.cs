using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.MicrosoftAes;
using VFSBase.Persistence.Coding.Strategies;

namespace VFSBaseTests.Coding
{
    [TestClass]
    public class EncryptionStrategiesTest
    {
        private static EncryptionOptions GetEncryptionOptions()
        {
            var key = new byte[32];
            var iv = new byte[16];

            var r = new Random(2);

            r.NextBytes(key);
            r.NextBytes(iv);

            return new EncryptionOptions(key, iv);
        }

        [TestMethod]
        public void TestMicrosoftEncryptionStrategy()
        {
            var s = new MicrosoftStreamEncryptionStrategy(GetEncryptionOptions());
            using (var ms = new MemoryStream())
            {
                var cryptedIn = s.DecorateToVFS(ms) as CryptoStream;
                var plainOut = s.DecorateToHost(ms);

                Assert.IsNotNull(cryptedIn);

                cryptedIn.WriteByte(1);
                cryptedIn.WriteByte(2);
                cryptedIn.WriteByte(3);
                cryptedIn.WriteByte(10);
                cryptedIn.Flush();
                cryptedIn.FlushFinalBlock();

                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(0x01, plainOut.ReadByte());
                Assert.AreEqual(0x02, plainOut.ReadByte());
                Assert.AreEqual(0x03, plainOut.ReadByte());
                Assert.AreEqual(0x0a, plainOut.ReadByte());

            }
        }

        [TestMethod]
        public void TestSelfMadeEncryptionStrategy()
        {
            var s = new SelfMadeAes256StreamEncryptionStrategy(GetEncryptionOptions());
            using (var ms = new MemoryStream())
            {
                var cryptedIn = s.DecorateToVFS(ms) as CryptoStream;
                var plainOut = s.DecorateToHost(ms);

                Assert.IsNotNull(cryptedIn);

                cryptedIn.WriteByte(1);
                cryptedIn.WriteByte(2);
                cryptedIn.WriteByte(3);
                cryptedIn.WriteByte(10);
                cryptedIn.Flush();
                cryptedIn.FlushFinalBlock();

                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(0x01, plainOut.ReadByte());
                Assert.AreEqual(0x02, plainOut.ReadByte());
                Assert.AreEqual(0x03, plainOut.ReadByte());
                Assert.AreEqual(0x0a, plainOut.ReadByte());

            }
        }

        [TestMethod]
        public void TestNullCodingStrategy()
        {
            var s = new NullStreamCodingStrategy();
            using (var ms = new MemoryStream())
            {
                Assert.AreSame(ms, s.DecorateToVFS(ms));
                Assert.AreSame(ms, s.DecorateToHost(ms));
            }
        }

        [TestMethod]
        public void TestSelfMadeSimpleStrategyFactory()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.SelfMadeSimple, StreamCompressionType.None));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms);
                var b = s.DecorateToHost(ms);
                Assert.AreNotSame(ms, a);
                Assert.AreNotSame(ms, b);
                Assert.IsTrue(a is CryptoStream);
                Assert.IsTrue(b is CryptoStream);
                var c = a as CryptoStream;
                var d = b as CryptoStream;
                Assert.AreEqual(false, c.CanRead);
                Assert.AreEqual(true, c.CanWrite);
                Assert.AreEqual(true, d.CanRead);
                Assert.AreEqual(false, d.CanWrite);
            }
        }

        public void TestSelfMadeCaesarStrategyFactory()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.SelfMadeCaesar, StreamCompressionType.None));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms);
                var b = s.DecorateToHost(ms);
                Assert.AreNotSame(ms, a);
                Assert.AreNotSame(ms, b);
                Assert.IsTrue(a is CryptoStream);
                Assert.IsTrue(b is CryptoStream);
                var c = a as CryptoStream;
                var d = b as CryptoStream;
                Assert.AreEqual(false, c.CanRead);
                Assert.AreEqual(true, c.CanWrite);
                Assert.AreEqual(true, d.CanRead);
                Assert.AreEqual(false, d.CanWrite);
            }
        }

        public void TestMicrosoftAesStrategyFactory()
        {
            var res = new StramStrategyResolver(new FileSystemOptions("", StreamEncryptionType.MicrosoftAes, StreamCompressionType.None));
            var s = res.ResolveStrategy();

            using (var ms = new MemoryStream())
            {
                var a = s.DecorateToVFS(ms);
                var b = s.DecorateToHost(ms);
                Assert.AreNotSame(ms, a);
                Assert.AreNotSame(ms, b);
                Assert.IsTrue(a is CryptoStream);
                Assert.IsTrue(b is CryptoStream);
                var c = a as CryptoStream;
                var d = b as CryptoStream;
                Assert.AreEqual(false, c.CanRead);
                Assert.AreEqual(true, c.CanWrite);
                Assert.AreEqual(true, d.CanRead);
                Assert.AreEqual(false, d.CanWrite);
            }
        }
    }
}
