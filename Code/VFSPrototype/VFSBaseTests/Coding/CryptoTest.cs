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
            public int DataAmount { get; set; }
            public int BufferSize { get; set; }
        }

        private static void TestAlgorithm(ICryptoTransform encryptor, ICryptoTransform decryptor)
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
                new TestConfig(100000, 100000),
            };

            foreach (var configuration in configurations)
            {
                ExecuteTest(encryptor, decryptor, configuration);
            }

        }

        private static void ExecuteTest(ICryptoTransform encryptor, ICryptoTransform decryptor, TestConfig configuration)
        {
            var original = new byte[configuration.DataAmount];
            var result = new byte[original.Length];

            new Random(1).NextBytes(original);

            using (var ms = new MemoryStream())
            {
                var s = new SelfMadeCryptoStream(ms, encryptor, SelfMadeCryptoStreamMode.Write);
                s.Write(original, 0, original.Length);
                s.FlushFinalBlock();

                ms.Seek(0, SeekOrigin.Begin);

                var ss = new SelfMadeCryptoStream(ms, decryptor, SelfMadeCryptoStreamMode.Read);
                var read = 0;
                var pos = 0;
                while ((read = ss.Read(result, pos, Math.Min(original.Length - read, configuration.BufferSize))) > 0)
                {
                    pos += read;
                }
                Console.WriteLine(pos);
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
            using (var r = Rijndael.Create())
            {
                r.CreateEncryptor();
                r.CreateDecryptor();
                var options = new EncryptionOptions(r.Key, r.IV);

                TestAlgorithm(
                    new SelfMadeAesCryptor(options.Key, options.InitializationVector, CryptoDirection.Encrypt),
                    new SelfMadeAesCryptor(options.Key, options.InitializationVector, CryptoDirection.Decrypt));
            }

        }

        [TestMethod]
        public void TestSelfMadeAesCryptor()
        {
            var options = GetEncryptionOptions();
            TestAlgorithm(
                new SelfMadeAesCryptor(options.Key, options.InitializationVector, CryptoDirection.Encrypt),
                new SelfMadeAesCryptor(options.Key, options.InitializationVector, CryptoDirection.Decrypt));

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
