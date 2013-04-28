using System;
using System.IO;
using System.IO.Compression;
using VFSBase.Exceptions;
using VFSBase.Implementation;

namespace VFSBase.Persistence.Coding.SelfMadeLz77
{
    /// <summary>
    /// LZ77 stream implementation
    /// 
    /// Algorithm (not implementation) sources:
    /// http://oldwww.rasip.fer.hr/research/compress/algorithms/fund/lz/lz77.html
    /// http://de.wikibooks.org/wiki/Datenkompression:_Verlustfreie_Verfahren:_W%C3%B6rterbuchbasierte_Verfahren:_LZ77
    /// http://michael.dipperstein.com/lzss/
    /// </summary>
    internal class SelfMadeLz77Stream : Stream
    {
        /// <summary>
        /// The underlying stream
        /// </summary>
        private readonly Stream _stream;
        
        /// <summary>
        /// The compression mode
        /// </summary>
        private readonly CompressionMode _mode;

        /// <summary>
        /// The cached buffer
        /// </summary>
        private readonly CircularBuffer<byte> _cachedBuffer = new CircularBuffer<byte>();

        #region Compression

        /// <summary>
        /// The current triple
        /// </summary>
        private Lz77Triple _currentTriple;

        #endregion


        #region Decompression

        /// <summary>
        /// The look ahead buffer
        /// </summary>
        private readonly CircularBuffer<byte> _lookAheadBuffer = new CircularBuffer<byte>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeLz77Stream"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
        public SelfMadeLz77Stream(Stream stream, CompressionMode mode)
        {
            _stream = stream;
            _mode = mode;
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            if (count <= 0) return 0;

            var i = ReadSingleByte();
            if (i == -1) return 0;

            buffer[offset] = (byte)i;
            return 1;
        }

        /// <summary>
        /// Reads a single byte.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="VFSException">Encoding error in stream</exception>
        private int ReadSingleByte()
        {
            while (_cachedBuffer.Length > Lz77Constants.WindowLength) _cachedBuffer.Remove(1);

            if (_currentTriple != null && _currentTriple.MatchLength == 0)
            {
                _cachedBuffer.Append(_currentTriple.Following);
                _currentTriple.MatchLength--;
                return _currentTriple.Following;
            }

            if (_currentTriple != null && _currentTriple.MatchLength > 0)
            {
                var ret = _cachedBuffer[(_cachedBuffer.Length - _currentTriple.MatchLocation)];
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
                _cachedBuffer.Append(_currentTriple.Following);
                _currentTriple.MatchLength--;
                return _currentTriple.Following;
            }

            // match
            var byteToReturn = _cachedBuffer[(_cachedBuffer.Length - _currentTriple.MatchLocation)];
            _cachedBuffer.Append(byteToReturn);
            _currentTriple.MatchLength--;
            return byteToReturn;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            for (var i = 0; i < count; i++)
            {
                WriteSingleByte(buffer[offset + i]);
            }
        }

        /// <summary>
        /// Writes the single byte.
        /// </summary>
        /// <param name="b">The b.</param>
        private void WriteSingleByte(byte b)
        {
            _lookAheadBuffer.Append(b);
            if (_lookAheadBuffer.Length <= Lz77Constants.LookAheadWindow) return;

            ProcessWrite();
        }

        /// <summary>
        /// Processes the write.
        /// </summary>
        private void ProcessWrite()
        {
            var current = _lookAheadBuffer.First;
            var matchLength = 1;

            var currentMatchLocationTmp = _cachedBuffer.IndexOf(current);
            if (currentMatchLocationTmp != -1 && _lookAheadBuffer.Length > 1)
            {
                var currentMatchLocation = currentMatchLocationTmp;
                matchLength++;

                while (matchLength < _lookAheadBuffer.Length)
                {
                    currentMatchLocationTmp = _cachedBuffer.IndexOf(_lookAheadBuffer.FromStart(matchLength), currentMatchLocation);

                    if (currentMatchLocationTmp == -1) break;

                    currentMatchLocation = currentMatchLocationTmp;
                    matchLength++;
                }

                _cachedBuffer.Append(_lookAheadBuffer.Remove(--matchLength));

                var offset = _cachedBuffer.Length - currentMatchLocation - matchLength;
                WriteTriple(offset, matchLength, _lookAheadBuffer.First);
            }
            else
            {
                WriteTriple(0, 0, current);
            }

            _cachedBuffer.Append(_lookAheadBuffer.Remove(1));
            while (_cachedBuffer.Length > Lz77Constants.WindowLength) _cachedBuffer.Remove(1);
        }

        /// <summary>
        /// Writes the triple.
        /// </summary>
        /// <param name="matchLoc">The match loc.</param>
        /// <param name="matchLength">Length of the match.</param>
        /// <param name="followed">The followed.</param>
        private void WriteTriple(int matchLoc, int matchLength, byte followed)
        {
            var writeByteBuffer = new byte[3];

            var concat = (matchLoc << 4) | matchLength;
            writeByteBuffer[0] = (byte)(concat >> 8);
            writeByteBuffer[1] = (byte)concat;
            writeByteBuffer[2] = followed;

            _stream.Write(writeByteBuffer, 0, 3);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            while (_lookAheadBuffer.Length > 0)
            {
                ProcessWrite();
            }
            _stream.Flush();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return _mode == CompressionMode.Decompress; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get { return _mode == CompressionMode.Compress; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        public override long Position { get; set; }
    }
}