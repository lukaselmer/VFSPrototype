using System.IO;
using System.IO.Compression;
using VFSBase.Persistence.Coding.General;

namespace VFSBase.Implementation
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