using System;
using System.IO;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding
{
    internal class MicrosoftStreamEncryptionStrategy : IStreamCodingStrategy
    {
        private readonly EncryptionOptions _options;

        public MicrosoftStreamEncryptionStrategy(EncryptionOptions options)
        {
            _options = options;
        }

        public Stream DecorateToVFS(Stream stream)
        {
            var rijAlg = Rijndael.Create();
            rijAlg.Key = _options.Key;
            rijAlg.IV = _options.InitializationVector;
            var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        public Stream DecorateToHost(Stream stream)
        {
            var rijAlg = Rijndael.Create();
            rijAlg.Key = _options.Key;
            rijAlg.IV = _options.InitializationVector;
            var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}