using System.IO;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.SelfMadeAes;

namespace VFSBase.Persistence.Coding.General
{
    internal class SelfMadeAes256StreamEncryptionStrategy : IStreamCodingStrategy
    {
        /// <summary>
        /// The options
        /// </summary>
        private readonly EncryptionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeAes256StreamEncryptionStrategy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SelfMadeAes256StreamEncryptionStrategy(EncryptionOptions options)
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
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}