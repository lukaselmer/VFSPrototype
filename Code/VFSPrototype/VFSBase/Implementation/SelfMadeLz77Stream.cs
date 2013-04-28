using System;
using System.IO;
using System.IO.Compression;

namespace VFSBase.Implementation
{
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