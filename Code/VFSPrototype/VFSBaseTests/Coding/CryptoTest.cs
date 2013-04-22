using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Practices.Unity.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence.Coding;

namespace VFSBaseTests.Coding
{
    [TestClass]
    public class CryptoTest
    {
        private struct TestConfig
        {
            public TestConfig(int dataAmount, int bufferSize)
                : this()
            {
                DataAmount = dataAmount;
                BufferSize = bufferSize;
            }
            public int DataAmount { get; private set; }
            public int BufferSize { get; private set; }
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
        }

        private static void TestAlgorithm(IEncryptorFactory factory)
        {
            var configurations = new[] {
                new TestConfig(3, 1),
                new TestConfig(1000, 1),
                new TestConfig(2000, 1),
                new TestConfig(1023, 1023),
                new TestConfig(1024, 1024),
                new TestConfig(1025, 1025),
                new TestConfig(2048, 2048),
                new TestConfig(10000, 1023),
                new TestConfig(10000, 1024),
                new TestConfig(10000, 1025),
                new TestConfig(100000, 100000)
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
                s.Write(original, 0, original.Length);
                s.FlushFinalBlock();

                ms.Seek(0, SeekOrigin.Begin);

                var ss = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                var read = 0;
                var pos = 0;
                while ((read = ss.Read(result, pos, Math.Min(original.Length - pos, configuration.BufferSize))) > 0)
                {
                    pos += read;
                }
            }

            for (var i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i], result[i]);
            }
        }

        private static EncryptionOptions GetEncryptionOptions()
        {
            using (var r = Rijndael.Create())
            {
                return new EncryptionOptions(r.Key, r.IV);
            }
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

            public ICryptoTransform Encryptor { get { return new SelfMadeAesCryptor(_options.Key, _options.InitializationVector, CryptoDirection.Encrypt); } }
            public ICryptoTransform Decryptor { get { return new SelfMadeAesCryptor(_options.Key, _options.InitializationVector, CryptoDirection.Decrypt); } }
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
    }
}
