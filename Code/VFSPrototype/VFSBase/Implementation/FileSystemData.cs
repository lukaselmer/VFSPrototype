using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class FileSystemData : IFileSystemData
    {
        private FileSystemData(string location, ulong diskSize)
        {
            Location = location;
            DiskSize = diskSize;

            if (File.Exists(Location)) ImportFromFile(location);
            else File.WriteAllText(Location, "");

            Root = new Folder();
        }

        private void ImportFromFile(string location)
        {
            // TODO: implement this
        }

        public FileSystemData(FileSystemOptions fileSystemOptions)
            : this(fileSystemOptions.Path, fileSystemOptions.Size)
        {

        }

        public ulong DiskSize { get; private set; }
        public ulong DiskFree { get; private set; }
        public ulong DiskOccupied { get; private set; }

        public string Location { get; private set; }

        internal Folder Root { get; private set; }

        public void Destroy()
        {
            File.Delete(Location);
        }
    }
}
