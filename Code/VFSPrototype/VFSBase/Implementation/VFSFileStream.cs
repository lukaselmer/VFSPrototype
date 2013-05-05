using System;
using System.Collections.Generic;
using System.IO;
using VFSBase.Exceptions;
using VFSBase.Interfaces;
using VFSBase.Persistence;

namespace VFSBase.Implementation
{
    internal class VFSFileStream : Stream
    {
        private readonly VFSFile _file;
        private readonly BlockParser _blockParser;
        private readonly FileSystemOptions _options;
        private readonly BlockAllocation _blockAllocation;
        private readonly BlockManipulator _blockManipulator;
        private readonly Persistence.Persistence _persistence;

        #region Writing

        private readonly byte[] _writeBuffer;
        private long _writeBufferPosition;
        private bool _overwriteNextBlock;
        private long _lastBlockNumber;

        #endregion

        #region Reading

        private IEnumerator<byte[]> _blocks;
        private byte[] _readBuffer;
        private byte[] _readBufferNext;
        private int _readBufferPosition;
        private bool _canRead = true;
        private bool _canWrite = true;

        #endregion


        public VFSFileStream(VFSFile file, BlockParser blockParser, FileSystemOptions options, BlockAllocation blockAllocation, BlockManipulator blockManipulator, Persistence.Persistence persistence)
        {
            _file = file;
            _blockParser = blockParser;
            _options = options;
            _blockAllocation = blockAllocation;
            _blockManipulator = blockManipulator;
            _persistence = persistence;

            _writeBuffer = new byte[_options.BlockSize];
        }

        public override void Flush()
        {
            if (!_canWrite) return;

            Persist(true);
            _file.LastBlockSize = (int)_writeBufferPosition;
            _persistence.Persist(_file);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            if (buffer.Length < count + offset) throw new ArgumentOutOfRangeException("buffer");

            if (!_canRead) return 0;
            if (_canWrite) _canWrite = false;

            if (_blocks == null) InitRead();

            var currentOffset = offset;
            var readCount = 0;

            while (count - readCount > 0)
            {
                ReadNextBlockIfNecessary();
                if (_readBuffer.Length == 0) break;

                while (_readBuffer.Length > _readBufferPosition)
                {
                    var amountToCopy = Math.Min(count - readCount, _readBuffer.Length - _readBufferPosition);
                    
                    if (amountToCopy == 0) break;

                    Array.Copy(_readBuffer, _readBufferPosition, buffer, currentOffset, amountToCopy);
                    _readBufferPosition += amountToCopy;
                    currentOffset += amountToCopy;
                    readCount += amountToCopy;
                }
            }

            return readCount;
        }

        private void InitRead()
        {
            _blocks = GetBlockList(_file).Blocks().GetEnumerator();
            _readBufferPosition = 0;
            _readBuffer = new byte[0];

            if (_blocks.MoveNext())
            {
                _readBufferNext = new byte[_blocks.Current.Length];
                _blocks.Current.CopyTo(_readBufferNext, 0);
            }
            else
            {
                _readBufferNext = null;
            }
        }

        private void ReadNextBlockIfNecessary()
        {
            if (_readBuffer.Length > _readBufferPosition) return;

            _readBuffer = _readBufferNext;
            _readBufferPosition = 0;

            _readBufferNext = _blocks.MoveNext() ? CopyBytes(_blocks.Current) : null;

            if (_readBuffer == null)
            {
                _readBuffer = new byte[0];
                _readBufferPosition = 0;
                return;
            }

            if (_readBufferNext != null) return;

            if (_file.LastBlockSize <= 0) return;

            var tmp = new byte[_file.LastBlockSize];
            Array.Copy(_readBuffer, 0, tmp, 0, _file.LastBlockSize);
            _readBuffer = tmp;
        }

        private static byte[] CopyBytes(byte[] toCopy)
        {
            var b = new byte[toCopy.Length];
            toCopy.CopyTo(b, 0);
            return b;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            if (!_canWrite) throw new VFSException("Stream is not writable");
            if (_canRead) _canRead = false;

            long written = 0;
            while (written < count)
            {
                var amountToCopy = Math.Min(count - written, Math.Min(_writeBuffer.Length - _writeBufferPosition, buffer.Length - written));
                Array.Copy(buffer, written + offset, _writeBuffer, _writeBufferPosition, amountToCopy);
                written += amountToCopy;
                _writeBufferPosition += amountToCopy;
                Persist();
            }
        }

        private void Persist(bool forceFlush = false)
        {
            // No need to write it to the disk
            if (_writeBufferPosition <= 0) return;

            // No need to write it to the disk yet (lazy, only writes to disk when forced or buffer is full)
            if (!forceFlush && _writeBufferPosition < _writeBuffer.Length) return;

            var overwriteThisBlock = _overwriteNextBlock;
            if (overwriteThisBlock)
            {
                _blockManipulator.WriteBlock(_lastBlockNumber, _writeBuffer);
            }
            else
            {
                var blockNr = _blockAllocation.Allocate();
                _blockManipulator.WriteBlock(blockNr, _writeBuffer);
                AppendBlockReference(_file, blockNr);
                _lastBlockNumber = blockNr;
            }

            _overwriteNextBlock = _writeBuffer.Length > _writeBufferPosition;

            if (!_overwriteNextBlock) _writeBufferPosition = 0;
        }

        private void AppendBlockReference(IIndexNode parentFolder, long reference)
        {
            GetBlockList(parentFolder).AddReference(reference);
        }

        private IBlockList GetBlockList(IIndexNode parentFolder)
        {
            return new BlockList(parentFolder, _blockAllocation, _options, _blockParser, _blockManipulator, _persistence);
        }

        public override bool CanRead
        {
            get { return _canRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position { get; set; }

        public override void Close()
        {
            Flush();
            base.Close();
        }

    }
}