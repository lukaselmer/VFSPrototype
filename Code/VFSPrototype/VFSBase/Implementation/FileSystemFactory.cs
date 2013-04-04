using System.IO;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal static class FileSystemFactory
    {
        private static FileSystem Create(FileSystemOptions options)
        {
            if (File.Exists(options.Location)) throw new VFSException("File already exists");

            using (var file = File.Open(options.Location, FileMode.CreateNew, FileAccess.Write))
            {
                options.Serialize(file);
            }

            return new FileSystem(options);
        }

        private static FileSystem Import(IFileSystemOptions options)
        {
            if (!File.Exists(options.Location)) throw new VFSException("File does not exist");

            FileSystemOptions newOptions;

            using (var file = File.Open(options.Location, FileMode.Open, FileAccess.ReadWrite))
            {
                newOptions = FileSystemOptions.Deserialize(file);
                newOptions.Location = options.Location;

                file.Seek(0, SeekOrigin.Begin);
                newOptions.Serialize(file);
            }

            return new FileSystem(newOptions);
        }

        public static IFileSystem CreateOrImport(FileSystemOptions options)
        {
            return File.Exists(options.Location) ? Import(options) : Create(options);
        }

        public static void Delete(IFileSystem fileSystem)
        {
            fileSystem.Dispose();
            File.Delete(fileSystem.FileSystemOptions.Location);
        }
    }
}
