using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

using VFSBase.Persistence;

namespace VFSBase.Implementation
{
    [Serializable]
    public class FileSystemOptions : IFileSystemOptions
    {
        private int _blockSize;

        public FileSystemOptions(string location, long diskSize)
        {
            Location = location;
            DiskSize = diskSize;
            BlockSize = (int)BinaryMathUtil.KB(8);
            MasterBlockSize = (uint)BinaryMathUtil.KB(32);
            NameLength = 255;
            BlockReferenceSize = 64;
            BlockAllocation = new BlockAllocation();

            // TODO: request key on startup? Don't save it in the file (attention, serialization!), that's a bad idea.
            using (var r = Rijndael.Create())
            {
                EncryptionKey = r.Key;
                EncryptionInitializationVector = r.IV;
            }
        }

        public string Location { get; set; }

        public long DiskSize { get; set; }

        public uint MasterBlockSize { get; set; }

        public int BlockReferenceSize { get; set; }

        public int StartOfDirectBlock { get { return 1 /*type byte*/+ NameLength; } }
        public int StartOfIndirectBlocks { get { return BlockSize - (3 * BlockReferenceSize); } }
        public int DirectBlocksSpace { get { return StartOfIndirectBlocks - StartOfDirectBlock; } }
        public int DirectBlocksAmount { get { return DirectBlocksSpace / BlockReferenceSize; } }

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
                if (value < (int)BinaryMathUtil.KB(2)) throw new VFSException("block size too small");
                _blockSize = value;
            }
        }

        public int IndirectionCountForIndirectNodes
        {
            get { return 2; }
        }

        public long DiskFree { get; private set; }
        public long DiskOccupied { get; private set; }

        public int NameLength { get; private set; }

        public int ReferencesPerIndirectNode { get { return BlockSize / BlockReferenceSize; } }

        public BlockAllocation BlockAllocation { get; set; }

        public long MaximumFileSize
        {
            get
            {
                return BinaryMathUtil.Power(ReferencesPerIndirectNode, IndirectionCountForIndirectNodes + 1) * BlockSize;
            }
        }

        public byte[] EncryptionKey { get; private set; }

        public byte[] EncryptionInitializationVector { get; private set; }
    }
}