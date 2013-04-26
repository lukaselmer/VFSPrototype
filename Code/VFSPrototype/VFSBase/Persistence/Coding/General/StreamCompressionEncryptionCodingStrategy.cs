using System.IO;

namespace VFSBase.Persistence.Coding.General
{
    public class StreamCompressionEncryptionCodingStrategy : IStreamCodingStrategy
    {
        /// <summary>
        /// The stream compression strategy
        /// </summary>
        private readonly IStreamCodingStrategy _streamCompressionStrategy;

        /// <summary>
        /// The stream encryption strategy
        /// </summary>
        private readonly IStreamCodingStrategy _streamEncryptionStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCompressionEncryptionCodingStrategy"/> class.
        /// </summary>
        /// <param name="streamCompressionStrategy">The stream compression strategy.</param>
        /// <param name="streamEncryptionStrategy">The stream encryption strategy.</param>
        public StreamCompressionEncryptionCodingStrategy(IStreamCodingStrategy streamCompressionStrategy, IStreamCodingStrategy streamEncryptionStrategy)
        {
            _streamCompressionStrategy = streamCompressionStrategy;
            _streamEncryptionStrategy = streamEncryptionStrategy;
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the virtual file system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToVFS(Stream stream)
        {
            // Pattern: Decorator
            return _streamEncryptionStrategy.DecorateToVFS(_streamCompressionStrategy.DecorateToVFS(stream));
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            // Pattern: Decorator
            return _streamEncryptionStrategy.DecorateToHost(_streamCompressionStrategy.DecorateToHost(stream));
        }
    }
}