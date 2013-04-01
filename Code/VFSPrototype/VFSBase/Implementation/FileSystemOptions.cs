using System.IO;
using VFSBase.Persistance;

namespace VFSBase.Implementation
{
    public class FileSystemOptions
    {
        public FileSystemOptions(string path, ulong size)
        {
            Path = path;
            Size = size;
            MasterBlockSize = (uint)BinaryMathUtil.MB(1);
        }

        public string Path { get; private set; }

        public ulong Size { get; private set; }

        public uint MasterBlockSize { get; private set; }

        public void Deserialize(FileStream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                Size = reader.ReadUInt64();
                MasterBlockSize = reader.ReadUInt32();
            }
        }

        public void Serialize(FileStream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Size);
                writer.Write(MasterBlockSize);
            }
        }

    }
}