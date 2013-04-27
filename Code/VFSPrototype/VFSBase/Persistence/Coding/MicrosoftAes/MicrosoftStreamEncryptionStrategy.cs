using System.IO;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.General;

namespace VFSBase.Persistence.Coding.MicrosoftAes
{
    /// <summary>
    /// The microsoft encryption strategy (recommended)
    /// </summary>
    internal class MicrosoftStreamEncryptionStrategy : IStreamCodingStrategy
    {
        /// <summary>
        /// The options
        /// </summary>
        private readonly EncryptionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftStreamEncryptionStrategy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public MicrosoftStreamEncryptionStrategy(EncryptionOptions options)
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
            var rijAlg = RijAlg();
            var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Rijs the alg.
        /// </summary>
        /// <returns></returns>
        private Rijndael RijAlg()
        {
            var rijAlg = Rijndael.Create();
            rijAlg.Key = _options.Key;
            rijAlg.IV = _options.InitializationVector;
            return rijAlg;
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            var rijAlg = RijAlg();
            var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}