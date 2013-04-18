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
        private int _currentBufferPosition;
        private byte[] _decryptedBlock;
        private int _decryptedBlockPosition;

        public SelfMadeCryptoStream(Stream stream, ICryptoTransform cryptor, SelfMadeCryptoStreamMode mode)
        {
            _stream = stream;
            _cryptor = cryptor;
            _mode = mode;
            _blockSize = mode == SelfMadeCryptoStreamMode.Read ? _cryptor.InputBlockSize : _cryptor.OutputBlockSize;
            _currentBuffer = new byte[_blockSize];
            _currentBufferPosition = 0;
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

            //var readCount = _stream.Read(_currentBuffer, offset, count);
            //TODO: implement this

            var toReadTotal = offset - count;
            var currentOffset = offset;

            while (toReadTotal > 0)
            {
                // Fill the current buffer
                while (_currentBufferPosition < _currentBuffer.Length)
                {
                    var shouldReadNow = _currentBuffer.Length - _currentBufferPosition;
                    var readCount = _stream.Read(_currentBuffer, _currentBufferPosition, shouldReadNow);

                    // Handle last block
                    if (readCount == 0)
                    {
                        if (_currentBufferPosition == 0) return 0;

                        if (_decryptedBlock == null)
                        {
                            _decryptedBlock = _cryptor.TransformFinalBlock(_currentBuffer, 0, _currentBufferPosition);
                            _decryptedBlockPosition = 0;
                        }

                        var toRead = Math.Min(_decryptedBlock.Length - _decryptedBlockPosition, toReadTotal);

                        if (toRead == 0) return count - toReadTotal;

                        Array.Copy(_decryptedBlock, _decryptedBlockPosition, buffer, currentOffset, toRead);
                        toReadTotal -= toRead;
                        currentOffset += toRead;
                    }

                    // Decrypted block empty, buffer is full => fill decrypted block
                    if (_decryptedBlockPosition <= _decryptedBlock.Length && _currentBuffer.Length <= _currentBufferPosition)
                    {
                        _decryptedBlock = new byte[_blockSize];
                        Array.Copy(_currentBuffer, 0, _decryptedBlock, 0, _blockSize);
                        _decryptedBlockPosition = 0;
                    }


                }


                // If current buffer is not full yet, fill the buffer
                if (_currentBufferPosition != _currentBuffer.Length) continue;
            }


            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite) throw new VFSException("Stream not writable");

            var bufferPosition = 0;

            while (bufferPosition < buffer.Length)
            {
                // Fill the current buffer
                var toCopy = Math.Min(count - bufferPosition, _currentBuffer.Length - _currentBufferPosition);
                Array.Copy(buffer, bufferPosition, _currentBuffer, _currentBufferPosition, toCopy);
                bufferPosition += toCopy;
                _currentBufferPosition += toCopy;

                // If current buffer is not full yet, fill the buffer
                if (_currentBufferPosition != _currentBuffer.Length) continue;

                // Encrypt block
                var outputBuffer = new byte[_currentBuffer.Length];
                _cryptor.TransformBlock(_currentBuffer, 0, _currentBuffer.Length, outputBuffer, 0);

                // Write encrypted block to stream
                _stream.Write(outputBuffer, 0, outputBuffer.Length);

                // Reset buffer position
                _currentBufferPosition = 0;
            }

            //_stream.Write(buffer, offset, count);
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
            if (_flushedFinalBlock) throw new VFSException("Final block flushed already");
            _flushedFinalBlock = true;

            if (_currentBufferPosition <= 0) return;

            var b = _cryptor.TransformFinalBlock(_currentBuffer, 0, _currentBufferPosition);
            _stream.Write(b, 0, b.Length);

            Flush();

            //throw new NotImplementedException();
        }
    }
}