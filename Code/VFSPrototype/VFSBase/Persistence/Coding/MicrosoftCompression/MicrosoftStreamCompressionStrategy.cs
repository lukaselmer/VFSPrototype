using System.IO;
using System.IO.Compression;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.Strategies;

namespace VFSBase.Persistence.Coding.MicrosoftCompression
{
    /// <summary>
    /// The microsoft compression strategy (recommended)
    /// </summary>
    public class MicrosoftStreamCompressionStrategy : IStreamCodingStrategy
    {
        /// <summary>
        /// Decorates the steam, so it can be written to the virtual file system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToVFS(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Compress);
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Decompress);
        }
    }
}