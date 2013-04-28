using System.IO;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.SelfMadeAes;
using VFSBase.Persistence.Coding.SelfMadeSimple;

namespace VFSBase.Implementation
{
    internal class SelfMadeSimpleStreamEncryptionStrategy : IStreamCodingStrategy
    {
        private readonly EncryptionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeSimpleStreamEncryptionStrategy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SelfMadeSimpleStreamEncryptionStrategy(EncryptionOptions options)
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
            var encryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, CryptoDirection.Encrypt);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            var decryptor = new SelfMadeSimpleCryptor(_options.Key, _options.InitializationVector, CryptoDirection.Decrypt);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}