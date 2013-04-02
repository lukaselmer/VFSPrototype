using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using VFSBase.Persistance;

namespace VFSBase.Implementation
{
    [Serializable]
    public class FileSystemOptions
    {
        public FileSystemOptions(string location, ulong diskSize)
        {
            Location = location;
            DiskSize = diskSize;
            MasterBlockSize = (uint)BinaryMathUtil.MB(1);
        }

        public string Location { get; set; }

        public ulong DiskSize { get; set; }

        public uint MasterBlockSize { get; set; }

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


        public ulong DiskFree { get; private set; }
        public ulong DiskOccupied { get; private set; }

    }
}