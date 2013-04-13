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
        private readonly Persistence _persistence;

        #region Writing

        private readonly byte[] _writeBuffer;
        private long _writeBufferPosition;
        private bool _overwriteNextBlock;
        private long _lastBlockNumber;

        #endregion

        #region Reading

        private IEnumerator<byte[]> _blocks;
        private readonly byte[] _readBuffer;
        private int _readBufferPosition;
        private bool _canRead = true;
        private bool _canWrite = true;

        #endregion


        public VFSFileStream(VFSFile file, BlockParser blockParser, FileSystemOptions options, BlockAllocation blockAllocation, BlockManipulator blockManipulator, Persistence persistence)
        {
            _file = file;
            _blockParser = blockParser;
            _options = options;
            _blockAllocation = blockAllocation;
            _blockManipulator = blockManipulator;
            _persistence = persistence;

            _writeBuffer = new byte[_options.BlockSize];
            _readBuffer = new byte[_options.BlockSize];
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
            if (!_canRead) return 0;
            if (_canWrite) _canWrite = false;

            if (_blocks == null)
            {
                _blocks = GetBlockList(_file).Blocks().GetEnumerator();
                _blocks.MoveNext();
                _readBufferPosition = 0;
            }

            if (buffer.Length < count + offset) throw new ArgumentOutOfRangeException("buffer");

            var readCount = 0;

            while (count - readCount > 0)
            {
                if (_readBufferPosition == _blocks.Current.LongLength)
                {
                    if (!_blocks.MoveNext())
                    {
                        _canRead = false;
                        _blocks = null;
                        break;
                    }
                    _readBufferPosition = 0;
                }

                var toCopyCount = Math.Min(count, _blocks.Current.Length - _readBufferPosition);

                Array.Copy(_blocks.Current, _readBufferPosition, buffer, offset + readCount, toCopyCount);
                readCount += toCopyCount;
                _readBufferPosition += toCopyCount;
            }

            return readCount;
        }

        public override void Write(byte[] toWrite, int offset, int count)
        {
            if (!_canWrite) throw new VFSException("Stream is not writable");
            if (_canRead) _canRead = false;

            long toWriteOffset = 0;
            while (toWriteOffset < toWrite.Length)
            {
                var amountToCopy = Math.Min(_writeBuffer.Length - _writeBufferPosition, toWrite.Length - toWriteOffset);
                Array.Copy(toWrite, toWriteOffset, _writeBuffer, _writeBufferPosition, amountToCopy);
                toWriteOffset += amountToCopy;
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