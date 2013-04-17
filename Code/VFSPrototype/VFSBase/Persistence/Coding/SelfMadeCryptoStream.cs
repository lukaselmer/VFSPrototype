using System;
using System.IO;
using System.Security.Cryptography;
using VFSBase.Exceptions;

namespace VFSBase.Persistence.Coding
{
    internal class SelfMadeCryptoStream : Stream
    {
        private Stream _stream;
        private readonly ICryptoTransform _cryptor;
        private readonly SelfMadeCryptoStreamMode _mode;
        private readonly int _blockSize;
        private byte[] _currentBuffer;
        private byte[] _nextBuffer;
        private bool _flushedFinalBlock = false;

        public SelfMadeCryptoStream(Stream stream, ICryptoTransform cryptor, SelfMadeCryptoStreamMode mode)
        {
            _stream = stream;
            _cryptor = cryptor;
            _mode = mode;
            _blockSize = mode == SelfMadeCryptoStreamMode.Read ? _cryptor.InputBlockSize : _cryptor.OutputBlockSize;
            _currentBuffer = new byte[_blockSize];
            _nextBuffer = new byte[_blockSize];

            if (_cryptor.InputBlockSize != _cryptor.OutputBlockSize)
            {
                throw new VFSException("Only encryption with same input and output block sizes are allowed in this implementation.");
            }

        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
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
            get { return false; }
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
            if (!disposing) return;

            if ((!_flushedFinalBlock) && (_mode == SelfMadeCryptoStreamMode.Write)) FlushFinalBlock();

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            base.Dispose(true);
        }

        public void FlushFinalBlock()
        {
            Flush();
            //throw new NotImplementedException();
        }
    }
}