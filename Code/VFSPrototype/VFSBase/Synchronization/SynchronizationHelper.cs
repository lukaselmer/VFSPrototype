using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using VFSBase.DiskServiceReference;
using VFSBase.Interfaces;

namespace VFSBase.Synchronization
{
    internal static class SynchronizationHelper
    {
        internal static DiskOptionsDto CalculateDiskOptions(IFileSystemOptions o)
        {
            byte[] serializedOptions;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, o);
                serializedOptions = ms.ToArray();
            }

            var diskOptions = new DiskOptionsDto
                {
                    BlockSize = o.BlockSize,
                    MasterBlockSize = o.MasterBlockSize,
                    SerializedFileSystemOptions = serializedOptions
                };
            return diskOptions;
        }
    }
}