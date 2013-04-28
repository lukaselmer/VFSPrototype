using System;
using System.IO;
using System.IO.Compression;
using VFSBase.Exceptions;
using VFSBase.Implementation;

namespace VFSBase.Persistence.Coding.SelfMadeLz77
{
    internal class SelfMadeLz77Stream : Stream
    {
        private readonly Stream _stream;
        private readonly CompressionMode _mode;

        private readonly CircularBuffer<byte> _cachedBuffer = new CircularBuffer<byte>();

        #region Compression

        private Lz77Triple _currentTriple;

        #endregion


        #region Decompression

        private readonly CircularBuffer<byte> _forwardBuffer = new CircularBuffer<byte>();

        #endregion

        public SelfMadeLz77Stream(Stream stream, CompressionMode mode)
        {
            _stream = stream;
            _mode = mode;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            if (count <= 0) return 0;

            var i = ReadSingleByte();
            if (i == -1) return 0;

            buffer[offset] = (byte)i;
            return 1;
        }

        private int ReadSingleByte()
        {
            while (_cachedBuffer.Length > Lz77Constants.WindowLength) _cachedBuffer.Remove(1);

            if (_currentTriple != null && _currentTriple.MatchLength == 0)
            {
                _cachedBuffer.Append(_currentTriple.followed);
                _currentTriple.MatchLength--;
                return _currentTriple.followed;
            }

            if (_currentTriple != null && _currentTriple.MatchLength > 0)
            {
                var ret = _cachedBuffer[(_cachedBuffer.Length - _currentTriple.MatchLoc)];
                _cachedBuffer.Append(ret);
                _currentTriple.MatchLength--;
                return ret;
            }

            var a1 = _stream.ReadByte();
            if (a1 == -1) return a1;

            var a2 = _stream.ReadByte();
            var a3 = _stream.ReadByte();
            if (a2 == -1 || a3 == -1) throw new VFSException("Encoding error in stream");

            _currentTriple = new Lz77Triple(a1, a2, a3);

            if (_currentTriple.MatchLength == 0)
            {
                // no match
                _cachedBuffer.Append(_currentTriple.followed);
                _currentTriple.MatchLength--;
                return _currentTriple.followed;
            }

            // match
            var byteToReturn = _cachedBuffer[(_cachedBuffer.Length - _currentTriple.MatchLoc)];
            _cachedBuffer.Append(byteToReturn);
            _currentTriple.MatchLength--;
            return byteToReturn;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            for (var i = 0; i < count; i++)
            {
                WriteSingleByte(buffer[offset + i]);
            }
        }

        private void WriteSingleByte(byte b)
        {
            _forwardBuffer.Append(b);
            if (_forwardBuffer.Length <= Lz77Constants.LookForwardWindow) return;

            ProcessWrite();
        }

        private void ProcessWrite()
        {
            var current = _forwardBuffer.First;
            var matchLength = 1;

            var currentMatchLocationTmp = _cachedBuffer.IndexOf(current);
            if (currentMatchLocationTmp != -1 && _forwardBuffer.Length > 1)
            {
                var currentMatchLocation = currentMatchLocationTmp;
                matchLength++;

                while (matchLength < _forwardBuffer.Length)
                {
                    currentMatchLocationTmp = _cachedBuffer.IndexOf(_forwardBuffer.FromStart(matchLength), currentMatchLocation);

                    if (currentMatchLocationTmp == -1) break;

                    currentMatchLocation = currentMatchLocationTmp;
                    matchLength++;
                }

                _cachedBuffer.Append(_forwardBuffer.Remove(--matchLength));

                var offset = _cachedBuffer.Length - currentMatchLocation - matchLength;
                WriteTriple(offset, matchLength, _forwardBuffer.First);
            }
            else
            {
                WriteTriple(0, 0, current);
            }

            _cachedBuffer.Append(_forwardBuffer.Remove(1));
            while (_cachedBuffer.Length > Lz77Constants.WindowLength) _cachedBuffer.Remove(1);
        }

        private void WriteTriple(int matchLoc, int matchLength, byte followed)
        {
            var writeByteBuffer = new byte[3];

            var concat = (matchLoc << 4) | matchLength;
            writeByteBuffer[0] = (byte)(concat >> 8);
            writeByteBuffer[1] = (byte)concat;
            writeByteBuffer[2] = followed;

            _stream.Write(writeByteBuffer, 0, 3);
        }

        public override void Flush()
        {
            while (_forwardBuffer.Length > 0)
            {
                ProcessWrite();
            }
            _stream.Flush();
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