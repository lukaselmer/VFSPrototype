using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Practices.Unity.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence.Coding;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.SelfMadeAes;
using VFSBase.Persistence.Coding.SelfMadeCaesar;
using VFSBase.Persistence.Coding.SelfMadeSimple;

namespace VFSBaseTests.Coding
{
    [TestClass]
    public class CryptoTest
    {
        private struct TestConfig
        {
            public TestConfig(int dataAmount, int readBufferSize, int writeBufferSize)
                : this()
            {
                DataAmount = dataAmount;
                ReadBufferSize = readBufferSize;
                WriteBufferSize = writeBufferSize;
            }
            public int DataAmount { get; private set; }
            public int ReadBufferSize { get; private set; }
            public int WriteBufferSize { get; private set; }
        }

        private interface IEncryptorFactory
        {
            void Init();
            ICryptoTransform Encryptor { get; }
            ICryptoTransform Decryptor { get; }
        }

        private class SimpleEncryptorFactory : IEncryptorFactory
        {
            private readonly ICryptoTransform _encryptor;
            private readonly ICryptoTransform _decryptor;

            public SimpleEncryptorFactory(ICryptoTransform encryptor, ICryptoTransform decryptor)
            {
                _encryptor = encryptor;
                _decryptor = decryptor;
            }

            public void Init() { }

            public ICryptoTransform Encryptor
            {
                get { return _encryptor; }
            }

            public ICryptoTransform Decryptor
            {
                get { return _decryptor; }
            }
        }

        private static void TestAlgorithm(ICryptoTransform encryptor, ICryptoTransform decryptor)
        {
            TestAlgorithm(new SimpleEncryptorFactory(encryptor, decryptor));
            encryptor.Dispose();
            decryptor.Dispose();
        }

        private static void TestAlgorithm(IEncryptorFactory factory)
        {
            var configurations = new[] {
                new TestConfig(1, 1, 1),
                new TestConfig(32, 1, 1000),
                new TestConfig(16, 15, 1000),
                new TestConfig(3, 1, 1000),
                new TestConfig(1000, 1, 1000),
                new TestConfig(2000, 1, 1000),
                new TestConfig(1023, 1023, 1000),
                new TestConfig(1024, 1024, 1000),
                new TestConfig(1025, 1025, 1000),
                new TestConfig(2048, 2048, 1000),
                new TestConfig(10000, 1023, 1000),
                new TestConfig(10000, 1024, 1000),
                new TestConfig(10000, 1025, 1000),
                //new TestConfig(100000, 100000, 1000) disabled, so tests execute faster
            };

            foreach (var configuration in configurations)
            {
                factory.Init();
                ExecuteTest(factory.Encryptor, factory.Decryptor, configuration);
            }
        }

        private static void ExecuteTest(ICryptoTransform encryptor, ICryptoTransform decryptor, TestConfig configuration)
        {
            var original = new byte[configuration.DataAmount];
            var result = new byte[original.Length];

            new Random(1).NextBytes(original);

            using (var ms = new MemoryStream())
            {
                var s = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                var writePos = 0;
                while (writePos < original.Length)
                {
                    var count = Math.Min(original.Length - writePos, original.Length);
                    s.Write(original, writePos, count);
                    writePos += count;
                }
                s.FlushFinalBlock();

                ms.Seek(0, SeekOrigin.Begin);

                var ss = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                int read;
                var pos = 0;
                while ((read = ss.Read(result, pos, Math.Min(original.Length - pos, configuration.ReadBufferSize))) > 0)
                {
                    pos += read;
                }
                var bb = new byte[1024];
                Assert.AreEqual(0, ss.Read(bb, 0, bb.Length));
            }

            for (var i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i], result[i]);
            }
        }

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
        public void TestMicrosoftAesCryptor()
        {
            TestAlgorithm(new RijandelEncryptorFactory());
        }


        private class RijandelEncryptorFactory : IEncryptorFactory
        {
            private Rijndael _r;

            public void Init()
            {
                if (_r != null) _r.Dispose();
                _r = Rijndael.Create();
            }

            public ICryptoTransform Encryptor { get { return _r.CreateEncryptor(); } }
            public ICryptoTransform Decryptor { get { return _r.CreateDecryptor(); } }
        }


        [TestMethod]
        public void TestSelfMadeAesCryptor()
        {
            var options = GetEncryptionOptions();
            TestAlgorithm(new SelfMadeAesCryptorFactory(options));
        }

        private class SelfMadeAesCryptorFactory : IEncryptorFactory
        {
            private readonly EncryptionOptions _options;

            public SelfMadeAesCryptorFactory(EncryptionOptions options)
            {
                _options = options;
            }

            public void Init()
            {
            }

            public ICryptoTransform Encryptor { get { return new SelfMadeAes256Cryptor(_options.Key, _options.InitializationVector, CryptoDirection.Encrypt); } }
            public ICryptoTransform Decryptor { get { return new SelfMadeAes256Cryptor(_options.Key, _options.InitializationVector, CryptoDirection.Decrypt); } }
        }

        [TestMethod]
        public void TestSelfMadeSimpleCryptor()
        {
            var options = GetEncryptionOptions();
            TestAlgorithm(
                new SelfMadeSimpleCryptor(options.Key, options.InitializationVector, CryptoDirection.Encrypt),
                new SelfMadeSimpleCryptor(options.Key, options.InitializationVector, CryptoDirection.Decrypt));

        }


        [TestMethod]
        public void TestSelfMadeCaesarCryptor()
        {
            TestAlgorithm(
                new SelfMadeCaesarCryptor(3, CryptoDirection.Encrypt),
                new SelfMadeCaesarCryptor(3, CryptoDirection.Decrypt));

            TestAlgorithm(
                new SelfMadeCaesarCryptor(7, CryptoDirection.Encrypt),
                new SelfMadeCaesarCryptor(7, CryptoDirection.Decrypt));

        }


        [TestMethod]
        public void SelfMadeCaesarCryptorReusable()
        {
            var selfMadeCaesarCryptor = new SelfMadeCaesarCryptor(3, CryptoDirection.Encrypt);
            Assert.AreEqual(true, selfMadeCaesarCryptor.CanReuseTransform);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestInputNullSelfMadeCaesarCryptor()
        {
            
            var selfMadeCaesarCryptor = new SelfMadeCaesarCryptor(3, CryptoDirection.Encrypt);
            selfMadeCaesarCryptor.TransformBlock(null, 0, 0, null, 0);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestOutputNullSelfMadeCaesarCryptor()
        {
            var selfMadeCaesarCryptor = new SelfMadeCaesarCryptor(3, CryptoDirection.Encrypt);
            Assert.AreEqual(true, selfMadeCaesarCryptor.CanReuseTransform);

            selfMadeCaesarCryptor.TransformBlock(new byte[0], 0, 0, null, 0);
        }
    }
}
