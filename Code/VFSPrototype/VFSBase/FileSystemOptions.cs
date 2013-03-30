namespace VFSBase
{
    public class FileSystemOptions
    {
        public readonly string Path;
        public readonly int Size;

        public FileSystemOptions(string path, int size)
        {
            Path = path;
            Size = size;
        }

    }
}