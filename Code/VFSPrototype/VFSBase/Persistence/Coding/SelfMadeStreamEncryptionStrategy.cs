using System.IO;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding
{
    internal class SelfMadeStreamEncryptionStrategy : IStreamCodingStrategy
    {
        private readonly EncryptionOptions _options;

        public SelfMadeStreamEncryptionStrategy(EncryptionOptions options)
        {
            _options = options;
        }

        public Stream DecorateToVFS(Stream stream)
        {
            var encryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, SelfMadeSimpleCryptor.CryptoDirection.Encrypt);
            //var encryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, SelfMadeSimpleCryptor.CryptoDirection.Encrypt);
            //var encryptor = new SelfMadeCaesarCryptor(_options.Key[0], SelfMadeCaesarCryptor.CryptoDirection.Encrypt);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        public Stream DecorateToHost(Stream stream)
        {
            var decryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, SelfMadeSimpleCryptor.CryptoDirection.Decrypt);
            //var decryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, SelfMadeSimpleCryptor.CryptoDirection.Decrypt);
            //var decryptor = new SelfMadeCaesarCryptor(_options.Key[0], SelfMadeCaesarCryptor.CryptoDirection.Decrypt);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}