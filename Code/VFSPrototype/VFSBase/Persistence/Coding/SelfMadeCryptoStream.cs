using System.IO;
using VFSBase.Exceptions;

namespace VFSBase.Persistence.Coding
{
    internal class SelfMadeCryptoStream : Stream
    {
        private readonly Stream _stream;
        private readonly SelfMadeAesCryptor _encryptor;
        private readonly SelfMadeCryptoStreamMode _mode;

        public SelfMadeCryptoStream(Stream stream, SelfMadeAesCryptor encryptor, SelfMadeCryptoStreamMode mode)
        {
            _stream = stream;
            _encryptor = encryptor;
            _mode = mode;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead) throw new VFSException("Stream not readable");
            var readCount = _stream.Read(buffer, offset, count);
            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite) throw new VFSException("Stream not writable");
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return _mode == SelfMadeCryptoStreamMode.Read; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _mode == SelfMadeCryptoStreamMode.Write; }
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position { get { return _stream.Position; } set { _stream.Position = value; } }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream.Dispose();
        }
    }
}