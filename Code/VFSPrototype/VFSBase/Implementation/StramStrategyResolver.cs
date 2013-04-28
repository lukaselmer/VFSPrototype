using System;
using System.IO;
using System.IO.Compression;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.MicrosoftAes;
using VFSBase.Persistence.Coding.MicrosoftCompression;

namespace VFSBase.Implementation
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

    internal class SelfMadeLz77StreamCompressionStrategy : IStreamCodingStrategy
    {
        public Stream DecorateToVFS(Stream stream)
        {
            return new SelfMadeLz77Stream(stream, CompressionMode.Compress);
        }

        public Stream DecorateToHost(Stream stream)
        {
            return new SelfMadeLz77Stream(stream, CompressionMode.Decompress);
        }
    }

    internal class SelfMadeLz77Stream : Stream
    {
        private readonly Stream _stream;
        private readonly CompressionMode _mode;

        public SelfMadeLz77Stream(Stream stream, CompressionMode mode)
        {
            _stream = stream;
            _mode = mode;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return _mode == CompressionMode.Decompress; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _mode == CompressionMode.Compress; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position { get; set; }
    }
}