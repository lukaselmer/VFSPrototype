using System.IO;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.SelfMadeAes;

namespace VFSBase.Persistence.Coding.General
{
    internal class SelfMadeStreamEncryptionStrategy : IStreamCodingStrategy
    {
        /// <summary>
        /// The options
        /// </summary>
        private readonly EncryptionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeStreamEncryptionStrategy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SelfMadeStreamEncryptionStrategy(EncryptionOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the virtual file system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToVFS(Stream stream)
        {
            var encryptor = new SelfMadeAes256Cryptor(_options.Key, _options.InitializationVector, CryptoDirection.Encrypt);
            //var encryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, SelfMadeSimpleCryptor.CryptoDirection.Encrypt);
            //var encryptor = new SelfMadeCaesarCryptor(_options.Key[0], SelfMadeCaesarCryptor.CryptoDirection.Encrypt);
            //return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            var decryptor = new SelfMadeAes256Cryptor(_options.Key, _options.InitializationVector, CryptoDirection.Decrypt);
            //var decryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, SelfMadeSimpleCryptor.CryptoDirection.Decrypt);
            //var decryptor = new SelfMadeCaesarCryptor(_options.Key[0], SelfMadeCaesarCryptor.CryptoDirection.Decrypt);
            //return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}