using System;
using System.IO;
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
        private byte[] _buffer;

        private long _bufferPosition;
        private bool _overwriteNextBlock = false;
        private long _lastBlockNumber = 0;

        public VFSFileStream(VFSFile file, BlockParser blockParser, FileSystemOptions options, BlockAllocation blockAllocation, BlockManipulator blockManipulator, Persistence persistence)
        {
            _file = file;
            _blockParser = blockParser;
            _options = options;
            _blockAllocation = blockAllocation;
            _blockManipulator = blockManipulator;
            _persistence = persistence;

            _buffer = new byte[_options.BlockSize];
        }

        public override void Flush()
        {
            Persist(true);
            _file.LastBlockSize = (int)_bufferPosition;
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
            throw new NotSupportedException();
        }

        public override void Write(byte[] toWrite, int offset, int count)
        {
            long toWriteOffset = 0;
            while (toWriteOffset < toWrite.Length)
            {
                var amountToCopy = Math.Min(_buffer.Length - _bufferPosition, toWrite.Length - toWriteOffset);
                Array.Copy(toWrite, toWriteOffset, _buffer, _bufferPosition, amountToCopy);
                toWriteOffset += amountToCopy;
                _bufferPosition += amountToCopy;
                Persist();
            }
        }

        private void Persist(bool forceFlush = false)
        {
            // No need to write it to the disk
            if (_bufferPosition <= 0) return;

            // No need to write it to the disk yet (lazy, only writes to disk when forced or buffer is full)
            if (!forceFlush && _bufferPosition < _buffer.Length) return;

            var overwriteThisBlock = _overwriteNextBlock;
            if (overwriteThisBlock)
            {
                _blockManipulator.WriteBlock(_lastBlockNumber, _buffer);
            }
            else
            {
                var blockNr = _blockAllocation.Allocate();
                _blockManipulator.WriteBlock(blockNr, _buffer);
                AppendBlockReference(_file, blockNr);
                _lastBlockNumber = blockNr;
            }

            _overwriteNextBlock = _buffer.Length > _bufferPosition;

            if (!_overwriteNextBlock) _bufferPosition = 0;
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
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
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