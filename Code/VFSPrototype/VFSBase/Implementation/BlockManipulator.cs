using System;
using System.IO;

namespace VFSBase.Implementation
{
    internal sealed class BlockManipulator :IDisposable
    {
        private FileStream _disk;
        private BinaryReader _diskReader;
        private BinaryWriter _diskWriter;
        private readonly FileSystemOptions _options;
        
        public BlockManipulator(FileSystemOptions options)
        {
            _options = options;

            _disk = new FileStream(_options.Location, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, _options.BlockSize, FileOptions.RandomAccess);
            _diskReader = new BinaryReader(_disk);
            _diskWriter = new BinaryWriter(_disk);
        }

        private void SeekToBlock(long blockNumber)
        {
            _disk.Seek(_options.MasterBlockSize + (blockNumber * _options.BlockSize), SeekOrigin.Begin);
        }

        public void WriteBlock(long blockNumber, byte[] block)
        {
            SeekToBlock(blockNumber);
            _diskWriter.Write(block);
        }

        public byte[] ReadBlock(long blockNumber)
        {
            SeekToBlock(blockNumber);
            var block = _diskReader.ReadBytes(_options.BlockSize);
            if (block.Length != _options.BlockSize) return new byte[_options.BlockSize];
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
    }
}