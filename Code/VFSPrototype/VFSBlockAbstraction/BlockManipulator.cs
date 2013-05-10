using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace VFSBlockAbstraction
{
    public sealed class BlockManipulator : IDisposable
    {
        private FileStream _disk;
        private BinaryReader _diskReader;
        private BinaryWriter _diskWriter;

        private readonly int _blockSize;
        private readonly int _masterBlockSize;
        private readonly string _location;

        public BlockManipulator(string location, int blockSize, int masterBlockSize)
        {
            _location = location;
            _blockSize = blockSize;
            _masterBlockSize = masterBlockSize;

            _disk = new FileStream(_location, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, blockSize, FileOptions.RandomAccess);
            _diskReader = new BinaryReader(_disk);
            _diskWriter = new BinaryWriter(_disk);
        }

        private void SeekToBlock(long blockNumber)
        {
            _disk.Seek(Offset(blockNumber), SeekOrigin.Begin);
        }

        private long Offset(long blockNumber)
        {
            return _masterBlockSize + (blockNumber * _blockSize);
        }

        public void WriteBlock(long blockNumber, byte[] block)
        {
            SeekToBlock(blockNumber);
            LockBlock(blockNumber);

            try
            {
                _diskWriter.Write(block);
                _diskWriter.Flush();
            }
            finally
            {
                UnlockBlock(blockNumber);
            }
        }

        public byte[] ReadBlock(long blockNumber)
        {
            SeekToBlock(blockNumber);
            LockBlock(blockNumber);
            try
            {
                var block = _diskReader.ReadBytes(_blockSize);
                return block.Length == _blockSize ? block : new byte[_blockSize];
            }
            finally
            {
                UnlockBlock(blockNumber);
            }
        }

        private void LockBlock(long blockNumber)
        {
            var tries = 0;
            while (!TryLockBlock(blockNumber) && (tries++) < 100) { }
            if (tries >= 100) throw new Exception("Unable to allocate block");
        }

        private bool TryLockBlock(long blockNumber)
        {
            try
            {
                _disk.Lock(Offset(blockNumber), _blockSize);
                return true;
            }
            catch (IOException) // Block is locked, try again
            {
                Thread.Sleep(10);
                return false;
            }
        }

        private void UnlockBlock(long blockNumber)
        {
            _disk.Unlock(Offset(blockNumber), _blockSize);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource.

            if (!disposing) return;

            // free managed resources
            if (_diskWriter != null) _diskWriter.Flush();
            if (_disk != null) _disk.Flush(true);

            if (_disk != null)
            {
                _disk.Flush(true);
                _disk.Dispose();
                _disk = null;
            }

            if (_diskReader != null)
            {
                _diskReader.Dispose();
                _diskReader = null;
            }

            if (_diskWriter != null)
            {
                _diskWriter.Dispose();
                _diskWriter = null;
            }
        }

        public void SaveConfig(byte[] options)
        {
            _disk.Seek(0, SeekOrigin.Begin);
            _disk.Write(options, 0, options.Length);
        }

        public void SaveConfig(object options)
        {
            _disk.Seek(0, SeekOrigin.Begin);

            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(_disk, options);
        }
    }
}