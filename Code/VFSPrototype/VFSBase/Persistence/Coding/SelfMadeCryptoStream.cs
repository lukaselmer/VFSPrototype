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
        private readonly byte[] _currentBuffer;
        private bool _flushedFinalBlock;
        private int _currentBufferPosition;
        private byte[] _decryptedBlock;
        private int _decryptedBlockPosition;
        private bool _lastBlockRead;

        public SelfMadeCryptoStream(Stream stream, ICryptoTransform cryptor, SelfMadeCryptoStreamMode mode)
        {
            _stream = stream;
            _cryptor = cryptor;
            _mode = mode;
            _blockSize = mode == SelfMadeCryptoStreamMode.Read ? _cryptor.InputBlockSize: _cryptor.OutputBlockSize;
            _currentBuffer = new byte[_blockSize];
            _currentBufferPosition = 0;
            
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
            if (buffer == null) throw new ArgumentNullException("buffer");

            if (!CanRead) throw new VFSException("Stream not readable");

            if (buffer.Length - offset < 0) throw new ArgumentException("offset not possible", "offset");

            if (count <= 0) return 0;

            var toReadTotal = count;
            var currentOffset = offset;

            if (_decryptedBlock == null)
            {
                _decryptedBlock = new byte[0];
                _decryptedBlockPosition = 0;
            }

            // while there is something to read
            while (toReadTotal > 0)
            {
                //while can read from decrypted block and something more to read
                while (toReadTotal > 0 && _decryptedBlock.Length > _decryptedBlockPosition)
                {
                    // read from decrypted block
                    var toCopy = Math.Min(toReadTotal, _decryptedBlock.Length - _decryptedBlockPosition);
                    Array.Copy(_decryptedBlock, _decryptedBlockPosition, buffer, currentOffset, toCopy);
                    toReadTotal -= toCopy;
                    _decryptedBlockPosition += toCopy;
                    currentOffset += toCopy;
                }

                if (toReadTotal <= 0) break;

                int read;
                // while current buffer is full or nothing more to read
                //   fill current buffer
                while ((_currentBuffer.Length - _currentBufferPosition) > 0 &&
                       (read = _stream.Read(_currentBuffer, _currentBufferPosition, _currentBuffer.Length - _currentBufferPosition)) > 0)
                {
                    _currentBufferPosition += read;
                }

                // if current buffer is full
                if ((_currentBuffer.Length - _currentBufferPosition) == 0)
                {
                    // It's not the last block
                    // Transform block, fill the decrypted block
                    var tmp = new byte[_currentBuffer.Length];
                    var readCrypted = _cryptor.TransformBlock(_currentBuffer, 0, tmp.Length, tmp, 0);

                    //if (readCrypted == 0) readCrypted = _cryptor.TransformBlock(_currentBuffer, 0, tmp.Length, tmp, 0);

                    //while (0 == (readCrypted = _cryptor.TransformBlock(_currentBuffer, 0, tmp.Length, tmp, 0))) { }

                    _decryptedBlock = new byte[readCrypted];
                    Array.Copy(tmp, 0, _decryptedBlock, 0, readCrypted);
                    _decryptedBlockPosition = 0;
                    _currentBufferPosition = 0;

                }
                else if (!_lastBlockRead)
                {
                    // Last block has been read
                    // Transform the last block, write it to de decrypted stream
                    _decryptedBlock = _cryptor.TransformFinalBlock(_currentBuffer, 0, _currentBufferPosition);
                    _decryptedBlockPosition = 0;
                    _lastBlockRead = true;
                }

                // If the decrypted block is fully read, there's nothing more to do
                if (_decryptedBlock.Length <= _decryptedBlockPosition) break;
            }

            return count - toReadTotal;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

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
        }
    }
}