using System.IO;
using VFSBase.Implementation;

namespace VFSBase.Persistence.Coding
{
    public class StreamCompressionEncryptionCodingStrategy : IStreamCodingStrategy
    {
        private readonly IStreamCodingStrategy _streamCompressionStrategy;
        private readonly IStreamCodingStrategy _streamEncryptionStrategy;

        public StreamCompressionEncryptionCodingStrategy(IStreamCodingStrategy streamCompressionStrategy, IStreamCodingStrategy streamEncryptionStrategy)
        {
            _streamCompressionStrategy = streamCompressionStrategy;
            _streamEncryptionStrategy = streamEncryptionStrategy;
        }

        public Stream DecorateToVFS(Stream stream)
        {
            // Pattern: Decorator
            return _streamEncryptionStrategy.DecorateToVFS(_streamCompressionStrategy.DecorateToVFS(stream));
        }

        public Stream DecorateToHost(Stream stream)
        {
            // Pattern: Decorator
            return _streamEncryptionStrategy.DecorateToHost(_streamCompressionStrategy.DecorateToHost(stream));
        }
    }
}