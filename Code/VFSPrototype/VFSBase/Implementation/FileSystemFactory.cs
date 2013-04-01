using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal static class FileSystemFactory
    {
        private static FileSystem Create(FileSystemOptions options)
        {
            if (File.Exists(options.Path)) throw new VFSException("File already exists");
            return new FileSystem(options);
        }

        private static FileSystem Import(FileSystemOptions options)
        {
            if (!File.Exists(options.Path)) throw new VFSException("File does not exist");

            using (var file = File.OpenRead(options.Path))
            {
                options.Deserialize(file);
            }

            return new FileSystem(options);
        }

        public static IFileSystem CreateOrImport(FileSystemOptions options)
        {
            return File.Exists(options.Path) ? Import(options) : Create(options);
        }
    }
}