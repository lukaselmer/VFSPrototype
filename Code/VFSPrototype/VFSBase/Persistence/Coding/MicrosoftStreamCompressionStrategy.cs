using System;
using System.IO;
using System.IO.Compression;

namespace VFSBase.Persistence.Coding
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