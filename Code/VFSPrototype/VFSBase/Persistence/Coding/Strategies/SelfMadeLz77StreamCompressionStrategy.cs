using System.IO;
using System.IO.Compression;
using VFSBase.Persistence.Coding.SelfMadeLz77;

namespace VFSBase.Persistence.Coding.Strategies
{
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
}