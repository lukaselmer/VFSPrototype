using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class FileSystem : IFileSystem
    {
        private FileSystem(string location, ulong diskSize)
        {
            Location = location;
            DiskSize = diskSize;

            if (File.Exists(Location)) throw new VFSException("File already exists!");

            File.WriteAllText(Location, "");

            Root = new Folder();
        }

        public FileSystem(FileSystemOptions fileSystemOptions) : this(fileSystemOptions.Path, fileSystemOptions.Size)
        {
            
        }

        public ulong DiskSize { get; private set; }
        public ulong DiskFree { get; private set; }
        public ulong DiskOccupied { get; private set; }

        public string Location { get; private set; }

        public Folder Root { get; private set; }

        public void Destroy()
        {
            File.Delete(Location);
        }
    }
}
