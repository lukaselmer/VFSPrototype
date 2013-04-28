using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

using VFSBase.Persistence;
using VFSBase.Persistence.Coding;
using VFSBase.Persistence.Coding.General;

namespace VFSBase.Implementation
{
    [Serializable]
    public class FileSystemOptions : IFileSystemOptions
    {
        private int _blockSize;

        [NonSerialized]
        private IStreamCodingStrategy _streamCodingStrategy;

        public FileSystemOptions(string location, long diskSize, StreamEncryptionType encryption = StreamEncryptionType.None, StreamCompressionType compression = StreamCompressionType.None)
        {
            Location = location;
            DiskSize = diskSize;
            Encryption = encryption;
            Compression = compression;
            BlockSize = (int)MathUtil.KB(8);
            MasterBlockSize = (uint)MathUtil.KB(32);
            NameLength = 255;
            BlockReferenceSize = 64;
            BlockAllocation = new BlockAllocation();
            IndirectionCountForIndirectNodes = 2;

            // TODO: request key (or part of key) on startup? Don't save it in the file (attention, serialization!), that's a bad idea.
            using (var r = Rijndael.Create())
            {
                EncryptionKey = r.Key;
                EncryptionInitializationVector = r.IV;
            }

            InitializeStreamCodingStrategy();
        }

        private void InitializeStreamCodingStrategy()
        {
            _streamCodingStrategy = new StramStrategyResolver(this).ResolveStrategy();
        }

        public string Location { get; set; }

        public long DiskSize { get; set; }
        public StreamEncryptionType Encryption { get; set; }
        public StreamCompressionType Compression { get; set; }

        public uint MasterBlockSize { get; set; }

        public int BlockReferenceSize { get; private set; }

        //public int StartOfDirectBlock { get { return 1 /*type byte*/+ NameLength; } }
        //public int StartOfIndirectBlocks { get { return BlockSize - (3 * BlockReferenceSize); } }
        //public int DirectBlocksSpace { get { return StartOfIndirectBlocks - StartOfDirectBlock; } }
        //public int DirectBlocksAmount { get { return DirectBlocksSpace / BlockReferenceSize; } }

        public static FileSystemOptions Deserialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream) as FileSystemOptions;
        }

        public void Serialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public int BlockSize
        {
            get
            {
                return _blockSize;
            }
            set
            {
                if (value < (int)MathUtil.KB(2)) throw new VFSException("block size too small");
                _blockSize = value;
            }
        }

        public int IndirectionCountForIndirectNodes { get; private set; }

        public long DiskFree
        {
            get
            {
                var s = Path.GetFullPath(Location);
                var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == Path.GetPathRoot(s));
                return driveInfo == null ? -1 : driveInfo.TotalFreeSpace;
            }
        }
        public long DiskOccupied { get { return _blockSize * BlockAllocation.OccupiedCount; } }

        public int NameLength { get; internal set; }

        public int ReferencesPerIndirectNode { get { return BlockSize / BlockReferenceSize; } }

        public BlockAllocation BlockAllocation { get; private set; }

        public long MaximumFileSize
        {
            get
            {
                return MathUtil.Power(ReferencesPerIndirectNode, IndirectionCountForIndirectNodes + 1) * BlockSize;
            }
        }

        // TODO: request key (or part of key) on startup? Don't save it in the file (attention, serialization!), that's a bad idea.
        // [NonSerialized]
        internal byte[] EncryptionKey { get; set; }

        // TODO: request key (or part of key) on startup? Don't save it in the file (attention, serialization!), that's a bad idea.
        // [NonSerialized]
        internal byte[] EncryptionInitializationVector { get; set; }

        public IStreamCodingStrategy StreamCodingStrategy
        {
            get
            {
                lock (GetType())
                {
                    if (_streamCodingStrategy == null) InitializeStreamCodingStrategy();
                }
                return _streamCodingStrategy;
            }
        }
    }
}