using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using VFSBase.Interfaces;
using VFSBase.Persistance;
using VFSBase.Persistance.Blocks;

namespace VFSBase.Implementation
{
    [Serializable]
    public class FileSystemOptions : IFileSystemOptions
    {
        public FileSystemOptions(string location, long diskSize)
        {
            Location = location;
            DiskSize = diskSize;
            MasterBlockSize = (uint)BinaryMathUtil.KB(1);
            NameLength = 255;
            BlockReferenceSize = 64;
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
            get { return (int)BinaryMathUtil.KB(4); }
        }

        public long DiskFree { get; private set; }
        public long DiskOccupied { get; private set; }

        public int NameLength { get; private set; }

        public int ReferencesPerIndirectNode { get { return BlockSize / BlockReferenceSize; } }
    }
}