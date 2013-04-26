using System.IO;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.General;

namespace VFSBase.Persistence.Coding.MicrosoftAes
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
            var rijAlg = RijAlg();
            var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        private Rijndael RijAlg()
        {
            var rijAlg = Rijndael.Create();
            rijAlg.Key = _options.Key;
            rijAlg.IV = _options.InitializationVector;
            return rijAlg;
        }

        public Stream DecorateToHost(Stream stream)
        {
            var rijAlg = RijAlg();
            var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }
}