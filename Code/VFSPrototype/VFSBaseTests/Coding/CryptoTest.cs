using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Persistence.Coding;

namespace VFSBaseTests.Coding
{
    [TestClass]
    public class CryptoTest
    {
        private static void TestAlgorithm(ICryptoTransform encryptor, ICryptoTransform decryptor)
        {
            var original = new byte[100000];
            var result = new byte[100000];

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
                while ((pos = ss.Read(result, pos, original.Length - read)) > 0)
                {
                    read += pos;
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
