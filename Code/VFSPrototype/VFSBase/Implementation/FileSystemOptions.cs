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
        public FileSystemOptions(string path, ulong size)
        {
            Path = path;
            Size = size;
            MasterBlockSize = (uint)BinaryMathUtil.MB(1);
        }

        public string Path { get; private set; }

        public ulong Size { get; set; }

        public uint MasterBlockSize { get; set; }

        public static FileSystemOptions Deserialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream) as FileSystemOptions;
            
            /*using (var reader = new BinaryReader(stream))
            {
                Size = reader.ReadUInt64();
                MasterBlockSize = reader.ReadUInt32();
            }*/
        }

        public void Serialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            //var writer = new BinaryWriter(stream);
            //writer.Write(Size);
            //writer.Write(MasterBlockSize);
        }


        public ulong DiskSize { get; private set; }
        public ulong DiskFree { get; private set; }
        public ulong DiskOccupied { get; private set; }

        public string Location { get; private set; }

    }
}