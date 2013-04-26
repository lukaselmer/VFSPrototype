using System.IO;
using System.IO.Compression;
using VFSBase.Persistence.Coding.General;

namespace VFSBase.Persistence.Coding.MicrosoftCompression
{
    public class MicrosoftStreamCompressionStrategy : IStreamCodingStrategy
    {
        public Stream DecorateToVFS(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Compress);
        }

        public Stream DecorateToHost(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Decompress);
        }
    }
}