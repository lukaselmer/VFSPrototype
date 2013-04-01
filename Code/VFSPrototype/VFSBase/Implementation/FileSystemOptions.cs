namespace VFSBase.Implementation
{
    public class FileSystemOptions
    {
        public readonly string Path;
        public readonly ulong Size;

        public FileSystemOptions(string path, ulong size)
        {
            Path = path;
            Size = size;
        }

    }
}