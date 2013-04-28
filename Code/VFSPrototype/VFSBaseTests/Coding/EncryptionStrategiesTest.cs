using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.MicrosoftAes;

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


    }
}
