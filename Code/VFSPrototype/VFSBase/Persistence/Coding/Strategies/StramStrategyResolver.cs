using System;
using VFSBase.Implementation;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.MicrosoftAes;
using VFSBase.Persistence.Coding.MicrosoftCompression;

namespace VFSBase.Persistence.Coding.Strategies
{
    internal class StramStrategyResolver
    {
        private readonly FileSystemOptions _options;

        public StramStrategyResolver(FileSystemOptions options)
        {
            _options = options;
        }

        public IStreamCodingStrategy ResolveStrategy()
        {
            return new StreamCompressionEncryptionCodingStrategy(CompressionStrategy(), EncryptionStrategy());
        }

        private IStreamCodingStrategy CompressionStrategy()
        {
            switch (_options.Compression)
            {
                case StreamCompressionType.None:
                    return new NullStreamCodingStrategy();
                case StreamCompressionType.MicrosoftDeflate:
                    return new MicrosoftStreamCompressionStrategy();
                case StreamCompressionType.SelfMadeLz77:
                    return new SelfMadeLz77StreamCompressionStrategy();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IStreamCodingStrategy EncryptionStrategy()
        {
            switch (_options.Encryption)
            {
                case StreamEncryptionType.None:
                    return new NullStreamCodingStrategy();
                case StreamEncryptionType.MicrosoftAes:
                    return new MicrosoftStreamEncryptionStrategy(new EncryptionOptions(_options.EncryptionKey, _options.EncryptionInitializationVector));
                case StreamEncryptionType.SelfMadeAes:
                    return new SelfMadeAes256StreamEncryptionStrategy(new EncryptionOptions(_options.EncryptionKey, _options.EncryptionInitializationVector));
                case StreamEncryptionType.SelfMadeCaesar:
                    return new SelfMadeCaesarStreamEncryptionStrategy(new EncryptionOptions(_options.EncryptionKey, _options.EncryptionInitializationVector));
                case StreamEncryptionType.SelfMadeSimple:
                    return new SelfMadeSimpleStreamEncryptionStrategy(new EncryptionOptions(_options.EncryptionKey, _options.EncryptionInitializationVector));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}