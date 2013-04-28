using System.IO;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.SelfMadeAes;
using VFSBase.Persistence.Coding.SelfMadeCaesar;

namespace VFSBase.Implementation
{
    internal class SelfMadeCaesarStreamEncryptionStrategy : IStreamCodingStrategy
    {
        private readonly EncryptionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeCaesarStreamEncryptionStrategy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SelfMadeCaesarStreamEncryptionStrategy(EncryptionOptions options)
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
            var encryptor = new SelfMadeCaesarCryptor(_options.Key[0] + _options.Key[1] + _options.Key[2] + _options.Key[3], CryptoDirection.Encrypt);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            var decryptor = new SelfMadeCaesarCryptor(_options.Key[0] + _options.Key[1] + _options.Key[2] + _options.Key[3], CryptoDirection.Decrypt);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}