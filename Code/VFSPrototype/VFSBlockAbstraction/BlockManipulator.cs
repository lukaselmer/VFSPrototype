using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

            //_disk = new FileStream(_location, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, blockSize, FileOptions.RandomAccess);
            _disk = new FileStream(_location, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, blockSize, FileOptions.RandomAccess);
            _diskReader = new BinaryReader(_disk);
            _diskWriter = new BinaryWriter(_disk);
        }

        private void SeekToBlock(long blockNumber)
        {
            _disk.Seek(_masterBlockSize + (blockNumber * _blockSize), SeekOrigin.Begin);
        }

        public void WriteBlock(long blockNumber, byte[] block)
        {
            SeekToBlock(blockNumber);
            _diskWriter.Write(block);
            _diskWriter.Flush();
        }

        public byte[] ReadBlock(long blockNumber)
        {
            SeekToBlock(blockNumber);
            var block = _diskReader.ReadBytes(_blockSize);
            if (block.Length != _blockSize) return new byte[_blockSize];
            return block;
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